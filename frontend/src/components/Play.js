import React, { useState, useEffect } from "react";
import "../styles/Play.css";

function Play() {
  const [move, setMove] = useState("");
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);

  // useEffect(() => {
  //   const fetchData = async () => {
  //     try {
  //       const response = await fetch(
  //         "https://jsonplaceholder.typicode.com/posts"
  //       );
  //       const result = await response.json();
  //       setData(result);
  //     } catch (error) {
  //       console.error("Error fetching data:", error);
  //     }
  //   };

  //   fetchData();
  // }, []);

  const fetchData = async () => {
    try {
      setLoading(true);

      setTimeout(async () => {
        const response = await fetch(
          "https://jsonplaceholder.typicode.com/posts"
        );
        const result = await response.json();

        setLoading(false);
        setData(result);
      }, 2000);
    } catch (error) {
      console.error("Error fetching data:", error);
    }
  };

  const postMove = async () => {
    try {
      /*
      {
      gameId: "12345"
      "playerMove": "e2e4"
      }
      */
    } catch (error) {
      console.error("Error");
    }
  };

  return (
    <div className="play-screen">
      {/* <img src={"/chess.jpg"} alt="Chess board" className="chess-board-image" /> */}
      <button
        onClick={(e) => {
          e.preventDefault();
          fetchData();
        }}
      >
        click mee
      </button>

      {loading && <h1>LOADING</h1>}

      {data.map((item) => (
        <>
          <p>{item.title}</p>
          <br></br>
        </>
      ))}
      <form className="move-form">
        <input
          type="text"
          value={move}
          onChange={(e) => setMove(e.target.value)}
          placeholder="B1 to B2"
        />
        <button
          type="submit"
          onClick={(e) => {
            e.preventDefault();
            //postMove
          }}
        >
          Submit Move
        </button>
      </form>
    </div>
  );
}

export default Play;
