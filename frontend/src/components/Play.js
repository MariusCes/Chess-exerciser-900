import React, { useState, useEffect } from "react";
import "../styles/Play.css";
import Board from "./Board";
import GameOver from "./GameOver";
import { Dropdown, DropdownButton, Button } from "react-bootstrap";

function Play() {
  const [move, setMove] = useState(""); // labelis tam judesiui kuri useris submittina
  const [moveList, setMoveList] = useState([]);
  const [loading, setLoading] = useState(false); // loading screen...? ar kazkur status update
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

  const createGame = async () => {
    setTimer(0);
    setMoveList([]);
    setHealth(100);
    const response = await fetch(
      "http://localhost:5030/api/chess/create-game",
      {
        method: "POST",
        body: JSON.stringify({
          aiDifficulty, // same as =>  aiDifficulty: aiDifficulty,
          memoryDifficulty,
        }),
        headers: {
          "Content-type": "application/json; charset=UTF-8",
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

  const formatTimer = (seconds) => {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${minutes.toString().padStart(2, "0")}:${secs
      .toString()
      .padStart(2, "0")}`;
  };

  return (
    <div className="relative min-h-screen overflow-x-hidden">
      <main
        className={`relative ${
          gameStatus ? "blur" : ""
        } transition-all duration-300`}
      >
        <div className="d-flex align-items-center justify-content-center mb-3">
          <label className="me-2">Game ID: {gameID}</label>
          <select
            className="form-select me-2"
            value={aiDifficulty}
            onChange={(e) => setAiDifficulty(e.target.value)}
            style={{ width: "120px" }}
          >
            <option value="">Select AI</option>
            <option value="1">Baby</option>
            <option value="2">Kid</option>
            <option value="3">Casual</option>
            <option value="4">Average MIF student</option>
            <option value="5">Competitive player</option>
            <option value="6">Professional player</option>
            <option value="7">Drunk Magnus Carlsen</option>
            <option value="8">AI overlord</option>
          </select>

          <select
            className="form-select me-2"
            value={memoryDifficulty}
            onChange={(e) => setMemoryDifficulty(e.target.value)}
            style={{ width: "160px" }}
          >
            <option value="">Select difficulty</option>
            <option value="1">ADHD 16 y.o</option>
            <option value="2">IT enjoyer</option>
            <option value="3">Literally a computer</option>
          </select>

          <button
            className="btn btn-secondary"
            onClick={createGame}
            disabled={!aiDifficulty || !memoryDifficulty}
          >
            Create Game
          </button>
        </div>
        <div className="container">
          <Board fen={fen} turnBlack={turnBlack} />
          <div>
            <form className="move-form">
              <input
                className="me-1"
                type="text"
                value={move}
                onChange={(e) => setMove(e.target.value)}
                placeholder="B1B2"
              />
              <button
                className="btn btn-secondary"
                type="submit"
                onClick={(e) => {
                  e.preventDefault();
                  postMove(move);
                  setMove("");
                }}
                disabled={!isGameCreated || gameStatus !== null}
              >
                Submit Move
              </button>
            </form>

              <div className="timer-container mt-2">
                <span className="timer">{formatTimer(timer)}</span>
              </div>

            <div className="health-bar-container">
              <div className="health-bar" style={{ width: `${health}%` }}></div>
            </div>
            <div className="move-list-container">
              <ul className="move-list">
                {moveList.map((move, index) => (
                  <li
                    key={index}
                    className={`move-item ${
                      index % 2 === 0 ? "your-move" : "bot-move"
                    }`}
                  >
                    {move}
                  </li>
                ))}
              </ul>
            </div>
            <button
              onClick={() => setTurnBlack(!turnBlack)}
              className="btn btn-secondary mt-2"
            >
              Toggle Piece Visibility
            </button>

            <div className="test-buttons mt-3">
              <button
                onClick={() => setGameStatus("win")}
                className="btn btn-success me-2"
              >
                Test Win
              </button>
              <button
                onClick={() => setGameStatus("draw")}
                className="btn btn-warning me-2"
              >
                Test Draw
              </button>
              <button
                onClick={() => setGameStatus("lose")}
                className="btn btn-danger"
              >
                Test Lose
              </button>

              <button
                onClick={() => decreaseHealth(10)}
                className="btn btn-warning ms-2"
              >
                Decrease Health
              </button>
            </div>
            <div className="test-buttons2 mt-3">
              <button onClick={mockCreateGame} className="btn btn-success me-2">
                Mock create game
              </button>
            </div>
          </div>
        </div>
      </main>
      {gameStatus && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <GameOver
            status={gameStatus}
            moveList={moveList}
            onPlayAgain={mockCreateGame}
            onClose={() => {
              setGameStatus(null);
              setIsGameCreated(false);
            }}
          />
        </div>
      )}
    </div>
  );
}

export default Play;
