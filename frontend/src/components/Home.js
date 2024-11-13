import React from "react";
import "../styles/Home.css";

function Home() {
  return (
    <div className="home text-center">
      <img
        src={"/ce9000.png"}
        alt="Chess Exerciser Logo"
        className="home-logo img-fluid rounded-circle"
      />
    </div>
  );
}

export default Home;
