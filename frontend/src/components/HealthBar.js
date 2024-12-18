import React from 'react';

export const HealthBar = ({ health }) => {
  return (
    <div className="health-bar-container">
      <div className="health-bar" style={{ width: `${health}%` }}></div>
    </div>
  );
};

export default HealthBar;