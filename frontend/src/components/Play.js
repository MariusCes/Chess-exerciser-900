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
  const [isLoading, setIsLoading] = useState(false);


  // This saves to localStorage
  useEffect(() => {
    if (isGameCreated) {
      sessionStorage.setItem('chessGameState', JSON.stringify({
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
  }, [gameID, fen, moveList, health, timer, turnBlack, aiDifficulty, memoryDifficulty, isGameCreated]);

  // On component load, restore the game state
  useEffect(() => {
    const savedGameState = sessionStorage.getItem('chessGameState');
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
    sessionStorage.removeItem('chessGameState');
    setGameID(null);
    setIsGameCreated(false);
    setMoveList([]);
    setHealth(100);
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

    setIsLoading(true);


    try {
      const response = await fetch(
        "http://localhost:5030/api/chess/create-game",
        {
          method: "POST",
          body: JSON.stringify({
            gameDifficulty: memoryDifficulty,
            aiDifficulty,
          }),
          headers: {
            "Content-type": "application/json; charset=UTF-8",
            Authorization: `Bearer ${token}`,
          },
        }
      );
      const data = await response.json();
      setGameStatus(null);
      setGameID(data.gameId);
      setIsGameCreated(true);
    } catch (error) {
      console.error("Error creating game:", error);
    } finally {
      setIsLoading(false);
    }
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
    const gameTime = `${Math.floor(timer / 3600)
      .toString()
      .padStart(2, "0")}:${Math.floor((timer % 3600) / 60)
        .toString()
        .padStart(2, "0")}:${(timer % 60).toString().padStart(2, "0")}`;
  
    setIsLoading(true);
  
    try {
      const response = await fetch(
        "http://localhost:5030/api/chess/" + gameID + "/move",
        {
          method: "POST",
          body: JSON.stringify({
            move: userMove.toLowerCase(),
            gameTime,
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
        setFen(data.fenPosition);
        setTurnBlack(data.turnBlack);

        if (data.wld !== undefined) {
          setGameStatus(
            data.wld === 1
              ? "win"
              : data.wld === 0
                ? "draw"
                : "lose"
          );
        }

      } else {
        setMove("Bad move!");
        decreaseHealth(10);
      }
    } catch (error) {
      console.error("Error posting move:", error);
    } finally {
      setIsLoading(false);
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
        {/* Loading Overlay */}
        {isLoading && (
          <div className="loading-overlay">
            <div className="spinner-border text-light" role="status">
              <span className="visually-hidden">Loading...</span>
            </div>
          </div>
        )}
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
            <MoveList moves={moveList}/>
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