import React, { useState, useEffect } from "react";
import "../styles/Play.css";

function Play() {
  const [move, setMove] = useState("");
  const [data, setData] = useState([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch(
          "https://jsonplaceholder.typicode.com/posts"
        );
        const result = await response.json();
        setData(result);
      } catch (error) {
        console.error("Error fetching data:", error);
      }
    };

    fetchData();
  }, []);

  return (
    <div className="play-screen">
      {/* <img src={"/chess.jpg"} alt="Chess board" className="chess-board-image" /> */}
      {data.map((item) => (
        <>
          {item.title} {item.description}
        </>
      ))}
      <form className="move-form">
        <input
          type="text"
          value={move}
          onChange={(e) => setMove(e.target.value)}
          placeholder="B1 to B2"
        />
        <button type="submit">Submit Move</button>
      </form>
    </div>
  );
}

export default Play;
