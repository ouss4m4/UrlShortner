import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useAuth } from "./hooks/useAuth";
import { useToast } from "./hooks/useToast";
import { Auth } from "./components/Auth";
import { Landing } from "./pages/Landing";
import { DashboardPage } from "./pages/DashboardPage";
import { useState } from "react";
import "./App.css";

function App() {
  const { user, isAuthenticated, login, logout } = useAuth();
  const { success, error: showError, ToastContainer } = useToast();
  const [showAuth, setShowAuth] = useState(false);

  const handleAuthSuccess = (authResponse: any) => {
    login(authResponse);
    setShowAuth(false);
    success(`Welcome, ${authResponse.username}!`);
  };

  const handleLogout = () => {
    logout();
    success("Logged out successfully");
  };

  return (
    <BrowserRouter>
      {showAuth && <Auth onSuccess={handleAuthSuccess} onCancel={() => setShowAuth(false)} />}

      <Routes>
        <Route
          path="/"
          element={
            isAuthenticated && user ? (
              <Navigate to="/dashboard" replace />
            ) : (
              <Landing onAuthClick={() => setShowAuth(true)} ToastContainer={ToastContainer} onSuccess={success} onError={showError} />
            )
          }
        />
        <Route
          path="/dashboard"
          element={
            isAuthenticated && user ? (
              <DashboardPage user={user} onLogout={handleLogout} ToastContainer={ToastContainer} onSuccess={success} onError={showError} />
            ) : (
              <Navigate to="/" replace />
            )
          }
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
