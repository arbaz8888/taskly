import { useEffect, useState } from "react";

export default function Quote() {
  const [text, setText] = useState<string>("");

  useEffect(() => {
    // Simple public API with CORS support
    fetch("https://api.quotable.io/random?tags=inspirational")
      .then((r) => r.ok ? r.json() : Promise.reject())
      .then((data) => setText(data.content))
      .catch(() => setText("Stay focused. Keep shipping.")); // fallback
  }, []);

  if (!text) return null;

  return (
    <div style={{
      marginTop: 8,
      padding: "6px 10px",
      border: "1px solid #eee",
      borderRadius: 6,
      fontSize: 14,
      fontStyle: "italic"
    }}>
      “{text}”
    </div>
  );
}
