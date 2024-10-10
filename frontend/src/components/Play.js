import React, { useState } from "react";
import "../styles/Play.css";

function Play() {
  const [move, setMove] = useState(""); // labelis tam judesiui kuri useris submittina
  const [moveList, setMoveList] = useState([]);
  const [loading, setLoading] = useState(false); // loading screen...? ar kazkur status update
  const [isGameCreated, setIsGameCreated] = useState(false); // jei nesukurtas zaidimas negali submittinti judesiu
  const [gameID, setGameID] = useState(""); // tas ID kuri atsiuncia

  const mockCreateGame = () => {
    const mockGameId = "777777"; // Mock game ID
    setGameID(mockGameId);
    setIsGameCreated(true);
    setMoveList([]); // Reset move list on new game creation
    console.log("Wow, the back has created a game ID without errors:", mockGameId);
  };

  const mockPostMove = (userMove) => {
    const mockBotMove = "E2E4"; // Mock bot move
    if (userMove !== "bad") {
      setMoveList((prevMoves) => [...prevMoves, userMove, mockBotMove]);
    } else {
      setMove("Bad move!");
    }
  };
  // atskiras component kuris butu uzloadinamas jei zaidimas sukurtas ir ten ir vyktu visas zaidimas, o siaip tai "create game" button ir vsio (checkai su "isGameCreated" butu)

  return (
    <div className="play-screen">
      <button
        type="submit"
        onClick={(e) => {
          e.preventDefault();
          mockCreateGame();
        }}
      >
        Create Game
      </button>

      <label>game ID: {gameID}</label>

      <form className="move-form">
        <input
          type="text"
          value={move}
          onChange={(e) => setMove(e.target.value)}
          placeholder="B1B2"
        />
        <button
          type="submit"
          onClick={(e) => {
            e.preventDefault();
            mockPostMove(move);
            setMove("");
          }}
        >
          Submit Move
        </button>
      </form>

      <ul>
        {" "}
        {/* po kolkas taip isveda judesius */}
        {moveList.map((move) => {
          return <li>{move}</li>;
        })}
      </ul>
    </div>
  );
}

export default Play;
