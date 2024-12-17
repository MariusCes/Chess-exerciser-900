import React, { useState, useEffect } from "react";
import "../styles/Play.css";
import Board from "./Board";
import GameOver from "./GameOver";
import Timer from "./Timer";
import HealthBar from "./HealthBar";
import MoveList from "./MoveList";
import GameControls from "./GameControls";
import DifficultySelectors from "./DifficultySelectors";
import TestButtons from "./TestButtons";
import LoginPrompt from "./LoginPrompt";

import { useAuth } from './AuthContext';

function Play() {

    const { token } = useAuth(); // is konteksto istraukta tokenas

  const [move, setMove] = useState(""); // labelis tam judesiui kuri useris submittina
  const [moveList, setMoveList] = useState([]);
  const [isGameCreated, setIsGameCreated] = useState(false); // jei nesukurtas zaidimas negali submittinti judesiu
  const [gameID, setGameID] = useState(""); // tas ID kuri atsiuncia
  const [fen, setFen] = useState(
    "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
  );
  const [turnBlack, setTurnBlack] = useState(false);
  const [aiDifficulty, setAiDifficulty] = useState(""); // State for selected difficulty
  const [memoryDifficulty, setMemoryDifficulty] = useState("");
  const [gameStatus, setGameStatus] = useState(null);
  const [health, setHealth] = useState(100);
  const [timer, setTimer] = useState(0);
  const [developerMode, setDeveloperMode] = useState(false);
  const [showLoginRequired, setShowLoginRequired] = useState(false);

  // This saves to localStorage
  useEffect(() => {
    if (isGameCreated) {
      localStorage.setItem('chessGameState', JSON.stringify({
        // Here you say which variables to keep an eye on for change
        gameID,
        fen,
        moveList,
        health,
        timer,
        turnBlack,
        aiDifficulty,
        memoryDifficulty,
        isGameCreated
      }));
    }
  }, [
    // Here you say what to write into the localStorage save
    gameID,
    fen,
    moveList,
    health,
    timer,
    turnBlack,
    aiDifficulty,
    memoryDifficulty,
    isGameCreated
  ]);

  // On component load, restore the game state
  useEffect(() => {
    const savedGameState = localStorage.getItem('chessGameState');
    if (savedGameState) {
      const parsedState = JSON.parse(savedGameState);
      setGameID(parsedState.gameID);
      setFen(parsedState.fen);
      setMoveList(parsedState.moveList);
      setHealth(parsedState.health);
      setTimer(parsedState.timer);
      setTurnBlack(parsedState.turnBlack);
      setAiDifficulty(parsedState.aiDifficulty);
      setMemoryDifficulty(parsedState.memoryDifficulty);
      setIsGameCreated(parsedState.isGameCreated);
    }
  }, []);

  const resetGame = () => {
    // Clear the localStorage to reset the game state
    localStorage.removeItem('chessGameState');

    // Reset the component's state to initial values  
    setGameID(null);
    setIsGameCreated(false);
    setMoveList([]);
    setHealth(100);;
    setTimer(0);
  };

  const togglePieceVisibility = () => {
    setTurnBlack(!turnBlack);
  };

  const createGame = async () => {

    if (!token) {
      setShowLoginRequired(true);
      return;
    }

    setTimer(0);
      setMoveList([]);
      setFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
      setHealth(health);

    const response = await fetch(
      "http://localhost:5030/api/chess/create-game",
      {
        method: "POST",
        body: JSON.stringify({
          gameDifficulty: memoryDifficulty, // same as =>  aiDifficulty: aiDifficulty,
          aiDifficulty,
        }),
        headers: {
            "Content-type": "application/json; charset=UTF-8",
            Authorization: `Bearer ${token}`, // uuuuuu yaaaa. token babyyy
        },
      }
    );
    const data = await response.json(); // unboxing
    setGameStatus(null);
    setGameID(data.gameId);
    setIsGameCreated(true);
  };

  const mockCreateGame = () => {
    setMoveList([]);
    setHealth(100);
    setGameStatus(null);
    setGameID(2024);
    setIsGameCreated(true);
    setTimer(0);
  };

  const postMove = async (userMove) => {
    const response = await fetch(
      "http://localhost:5030/api/chess/" + gameID + "/move",
      {
        method: "POST",
        body: JSON.stringify({
          move: userMove.toLowerCase(),
        }),
        headers: {
            "Content-type": "application/json; charset=UTF-8",
            Authorization: `Bearer ${token}`,
        },
      }
    );

    const data = await response.json();
    if (data.wrongMove === false) {
      setMoveList((prevMoves) => [...prevMoves, userMove, data.botMove]);
      setFen(data.fenPosition); // to be tested
      setTurnBlack(data.turnBlack); // to be tested???
    } else {
      setMove("Bad move!");
      decreaseHealth(10) // visada po 1 health nuima
    }
  };

  const decreaseHealth = (amount) => {
    setHealth((prevHealth) => {
      const newHealth = prevHealth - amount;
      if (newHealth <= 0) {
        setGameStatus("lose");
        return 0;
      }
      return newHealth;
    });
  };

  useEffect(() => {
    let interval;

    if (isGameCreated && !gameStatus) {
      interval = setInterval(() => {
        setTimer((prevTimer) => prevTimer + 1);
      }, 1000);
    }

    return () => {
      clearInterval(interval); // Stops the timer, but does not reset it
    };
  }, [isGameCreated, gameStatus]);

  return (
    <div className="relative min-h-screen overflow-x-hidden pt-5">
      <main
        className={`relative ${gameStatus ? "blur" : ""
          } transition-all duration-300`}
      >
        <DifficultySelectors
          aiDifficulty={aiDifficulty}
          setAiDifficulty={setAiDifficulty}
          memoryDifficulty={memoryDifficulty}
          setMemoryDifficulty={setMemoryDifficulty}
          createGame={createGame}
          setHealth={setHealth}
        />
        <div className="container">
          <Board fen={fen} turnBlack={turnBlack} />
          <div>
            <GameControls
              move={move}
              setMove={setMove}
              postMove={postMove}
              isGameCreated={isGameCreated}
              gameStatus={gameStatus}
              togglePieceVisibility={togglePieceVisibility}
            />
            <Timer seconds={timer} />
            <HealthBar health={health} />
            <MoveList moves={moveList} developerMode={developerMode} />
            <TestButtons
              setGameStatus={setGameStatus}
              decreaseHealth={decreaseHealth}
              mockCreateGame={mockCreateGame}
              resetGame={resetGame}
              developerMode={developerMode}
              togglePieceVisibility={togglePieceVisibility}
            />
            <label className="flex items-center space-x-2">
              <input
                type="checkbox"
                checked={developerMode}
                onChange={(e) => setDeveloperMode(e.target.checked)}
              />
              <span>Developer Mode</span>
            </label>
          </div>
        </div>
      </main>

      {gameStatus && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <GameOver
            status={gameStatus}
            moveList={moveList}
            onPlayAgain={createGame}
            onClose={() => {
              setGameStatus(null);
              setIsGameCreated(false);
            }}
          />
        </div>
      )}
          {showLoginRequired && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <LoginPrompt
            onClose={() => setShowLoginRequired(false)}
          />
        </div>
      )}
    </div>
  );
}

export default Play;