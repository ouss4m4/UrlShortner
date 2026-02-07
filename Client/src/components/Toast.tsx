import { useEffect } from "react";
import { cn } from "../lib/utils";

export type ToastType = "success" | "error" | "info";

interface ToastProps {
  message: string;
  type?: ToastType;
  duration?: number;
  onClose: () => void;
}

export function Toast({ message, type = "info", duration = 3000, onClose }: ToastProps) {
  useEffect(() => {
    const timer = setTimeout(onClose, duration);
    return () => clearTimeout(timer);
  }, [duration, onClose]);

  const typeStyles = {
    success: "bg-green-50 dark:bg-green-950 border-green-500 text-green-900 dark:text-green-100",
    error: "bg-red-50 dark:bg-red-950 border-red-500 text-red-900 dark:text-red-100",
    info: "bg-blue-50 dark:bg-blue-950 border-blue-500 text-blue-900 dark:text-blue-100",
  };

  const icons = {
    success: "✓",
    error: "✕",
    info: "ℹ",
  };

  return (
    <div
      className={cn(
        "fixed top-4 right-4 z-50 px-4 py-3 rounded-lg border-l-4 shadow-lg animate-in slide-in-from-top-5 duration-300",
        typeStyles[type],
      )}
    >
      <div className="flex items-center gap-3">
        <span className="text-xl font-bold">{icons[type]}</span>
        <p className="font-medium">{message}</p>
        <button onClick={onClose} className="ml-4 hover:opacity-70 transition-opacity font-bold">
          ×
        </button>
      </div>
    </div>
  );
}
