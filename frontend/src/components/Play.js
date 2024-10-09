import React, { useState } from "react";
import "../styles/Play.css";

function Play() {
  const [move, setMove] = useState(""); // labelis tam judesiui kuri useris submittina
  const [moveList, setMoveList] = useState([]);
  const [loading, setLoading] = useState(false); // loading screen...? ar kazkur status update
  const [isGameCreated, setIsGameCreated] = useState(false); // jei nesukurtas zaidimas negali submittinti judesiu
  const [gameID, setGameID] = useState(""); // tas ID kuri atsiuncia

  const createGame = async () => {
    setMoveList([]);
    const response = await fetch("http://localhost:5030/api/chess/create-game");
    const data = await response.json();
    setGameID(data.gameId);
    setIsGameCreated(true);
  };

  const postMove = async (userMove) => {
    const response = await fetch(
      "http://localhost:5030/api/chess/" + gameID + "/move",
      {
        method: "POST",
        body: JSON.stringify({
          move: userMove,
          id: gameID,
        }),
        headers: {
          "Content-type": "application/json; charset=UTF-8",
        },
      }
    );

    const data = await response.json();
    if (data.wrongMove === false) {
      setMoveList((prevMoves) => [...prevMoves, userMove, data.botMove]);
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
          createGame();
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
            postMove(move);
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
