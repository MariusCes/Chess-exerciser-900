import React, { useState, useEffect } from "react";
import "../styles/Play.css";

function Play() {
  const [move, setMove] = useState("");
  const [moveList, setMoveList] = useState([]);
  const [loading, setLoading] = useState(false);

  const postMove = async (userMove) => {
    try {
      setLoading(true);
      /*
      {
      gameId: "12345"
      "playerMove": "e2e4"
      }
      */
    } catch (error) {
      setLoading(false);
      console.error("Error");
    }
  };

  return (
    <div className="play-screen">
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
        {moveList.map((move) => {
          <li>move</li>;
        })}
      </ul>
    </div>
  );
}

export default Play;
