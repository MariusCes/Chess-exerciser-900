import React, { useEffect, useState } from "react";
import "../styles/Home.css";

function Home() {
  const [show, setShow] = useState(false);

  useEffect(() => {
    setShow(true);
  }, []);

  return (
    <div className="home-container">
      <div className={`title-container ${show ? 'show' : ''}`}>
        <div className="chess-text">CHESS</div>
        <div className="exerciser-text">EXERCISER</div>
        <div className="number-text">900</div>
      </div>
    </div>
  );
}

export default Home;