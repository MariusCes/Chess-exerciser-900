import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import '../styles/Login.css';

const Register = () => {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [notification, setNotification] = useState(null);
    const navigate = useNavigate();

    const showNotification = (message, type) => {
        setNotification({ message, type });
        
        // Automatically clear notification after 3 seconds
        setTimeout(() => {
            setNotification(null);
        }, 3000);
    };

    const handleRegister = async (e) => {
        e.preventDefault();

        if (password !== confirmPassword) {
            showNotification("Passwords do not match!", "error");
            return;
        }

        // Start loading
        setIsLoading(true);

        try {
            const response = await fetch(
                "http://localhost:5030/api/chess/register",
                {
                    method: "POST",
                    body: JSON.stringify({
                        UserName: username,
                        Email: email,
                        Password: password,
                        ConfirmPassword: confirmPassword
                    }),
                    headers: {
                        "Content-type": "application/json",
                    },
                }
            );

            const data = await response.json();

            // Simulate a minimum loading time
            await new Promise(resolve => setTimeout(resolve, 1000));

            if (data.message !== null) {
                showNotification("Registration successful! Redirecting...", "success");
                
                // Redirect after a short delay to show the success message
                setTimeout(() => {
                    navigate("/login");
                }, 2000);
            } else {
                showNotification("Something went wrong. Please try again.", "error");
            }
        } catch (error) {
            showNotification("An error occurred. Please try again.", "error");
        } finally {
            // Stop loading
            setIsLoading(false);
        }
    };

    return (
        <div className="login-container">
            {notification && (
                <div 
                    className={`notification ${notification.type}`}
                    style={{
                        position: 'fixed',
                        top: '20px',
                        left: '50%',
                        transform: 'translateX(-50%)',
                        zIndex: 1000,
                        padding: '10px 20px',
                        borderRadius: '5px',
                        color: 'white',
                        backgroundColor: notification.type === 'success' 
                            ? 'rgba(40, 167, 69, 0.8)' 
                            : 'rgba(220, 53, 69, 0.8)',
                        border: `1px solid ${notification.type === 'success' ? '#28a745' : '#dc3545'}`,
                        boxShadow: '0 4px 6px rgba(0,0,0,0.1)',
                        transition: 'all 0.3s ease'
                    }}
                >
                    {notification.message}
                </div>
            )}

            {isLoading && (
                <div className="loading-overlay">
                    <div className="spinner-border text-light" role="status">
                        <span className="visually-hidden">Loading...</span>
                    </div>
                </div>
            )}

            <div className="login-card">
                <h2 className="login-title">JOIN US, <br /> FUTURE CHESS MASTER</h2>
                <form onSubmit={handleRegister}>
                    <div className="mb-3">
                        <input
                            type="text"
                            className="form-control login-input"
                            placeholder="Username"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                        />
                    </div>
                    <div className="mb-3">
                        <input
                            type="email"
                            className="form-control login-input"
                            placeholder="Email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </div>
                    <div className="mb-3">
                        <input
                            type={showPassword ? "text" : "password"}
                            className="form-control login-input"
                            placeholder="Password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </div>
                    <div className="mb-3">
                        <input
                            type={showPassword ? "text" : "password"}
                            className="form-control login-input"
                            placeholder="Confirm Password"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                        />
                        <div className="d-flex align-items-center">
                            <input
                                type="checkbox"
                                className="form-check-input"
                                id="showPassword"
                                checked={showPassword}
                                onChange={() => setShowPassword(!showPassword)}
                                style={{ marginTop: 20 }}
                            />
                            <label
                                htmlFor="showPassword"
                                className="text-muted small"
                                style={{
                                    fontSize: '0.7rem',
                                    cursor: 'pointer',
                                    lineHeight: 1,
                                    marginBottom: 0
                                }}
                            >
                                Show password
                            </label>
                        </div>
                    </div>
                    <button
                        type="submit"
                        className="btn w-100 mb-3 login-btn"
                    >
                        Register
                    </button>
                    <div className="text-center">
                        <Link
                            to="/login"
                            className="login-link"
                        >
                            Already have an account? Login
                        </Link>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default Register;