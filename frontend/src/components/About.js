import React from 'react';
import '../styles/About.css';

function About() {
  return (
    <div className="about-section">
      <h1>About Chess Exerciser</h1>
      <p>
        BNW Presents: Chess Exerciser 9000! (Now available on your personal computer!)
        🚨 Attention all chess lovers! 🚨 Do you dream of becoming a Grandmaster? Tired
        of forgetting those winning moves? Let BNW's Chess Exerciser 9000 sharpen your
        mind and help you remember every move like never before!
      </p>
      <p>
        Whether you're a beginner or an advanced player, Chess Exerciser is designed
        to sharpen your skills, offering an exciting new twist on classic chess.
      </p>
      <img src={"/fruit.jpg"} alt="Fruit" className="about-image" />
    </div>
  );
}

export default About;
