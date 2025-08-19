import Quote from "./components/Quote";
import { useEffect, useState } from "react";

type TaskItem = { id: number; title: string; isComplete: boolean; workspaceId: number };

function Login({ onAuthed }: { onAuthed: (token: string) => void }) {
  const [username, setUsername] = useState("demo");
  const [password, setPassword] = useState("pass123");
  const [msg, setMsg] = useState("");

  async function login(e: React.FormEvent) {
    e.preventDefault();
    setMsg("...");
    const res = await fetch("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, password }),
    });
    if (!res.ok) {
      setMsg("Login failed");
      return;
    }
    const data = await res.json();
    localStorage.setItem("taskly_token", data.token);
    onAuthed(data.token);
  }

  return (
    <div style={{ maxWidth: 360, margin: "40px auto", fontFamily: "sans-serif" }}>
      <h2>Taskly – Login</h2>
      <form onSubmit={login} style={{ display: "grid", gap: 8 }}>
        <input
          placeholder="username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />
        <input
          placeholder="password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <button type="submit">Login</button>
        {msg && <small>{msg}</small>}
      </form>
      <p style={{ marginTop: 12 }}>
        Don’t have user? It’s already seeded workspace=1. You can register via API later.
      </p>
    </div>
  );
}

function Tasks({ token, onLogout }: { token: string; onLogout: () => void }) {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [title, setTitle] = useState("");
  const [loading, setLoading] = useState(false);
  const auth = { Authorization: `Bearer ${token}` };

  async function load() {
    setLoading(true);
    const res = await fetch("/tasks/", { headers: auth });
    const data = await res.json();
    setTasks(data);
    setLoading(false);
  }

  async function createTask(e: React.FormEvent) {
    e.preventDefault();
    if (!title.trim()) return;
    const res = await fetch("/tasks/", {
      method: "POST",
      headers: { "Content-Type": "application/json", ...auth },
      body: JSON.stringify({ title }),
    });
    if (res.ok) {
      setTitle("");
      await load();
    }
  }

  async function toggle(id: number) {
    const res = await fetch(`/tasks/${id}/toggle`, {
      method: "POST",
      headers: auth,
    });
    if (res.ok) await load();
  }

  useEffect(() => {
    load();
  }, []);

  return (
    <div style={{ maxWidth: 520, margin: "40px auto", fontFamily: "sans-serif" }}>
      <header style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <div>
    <h2>Taskly – My Tasks</h2>
    <Quote />
  </div>
        <button
          onClick={() => {
            localStorage.removeItem("taskly_token");
            onLogout();
          }}
        >
          Logout
        </button>
      </header>

      <form onSubmit={createTask} style={{ display: "grid", gridTemplateColumns: "1fr auto", gap: 8 }}>
        <input
          placeholder="New task title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
        <button type="submit">Add</button>
      </form>

      {loading ? (
        <p>Loading…</p>
      ) : tasks.length === 0 ? (
        <p>No tasks yet.</p>
      ) : (
        <ul style={{ listStyle: "none", padding: 0, marginTop: 12 }}>
          {tasks.map((t) => (
            <li
              key={t.id}
              style={{
                display: "flex",
                alignItems: "center",
                gap: 8,
                padding: "6px 0",
                borderBottom: "1px solid #eee",
              }}
            >
              <input
                type="checkbox"
                checked={t.isComplete}
                onChange={() => toggle(t.id)}
              />
              <span style={{ textDecoration: t.isComplete ? "line-through" : "none" }}>
                {t.title}
              </span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default function App() {
  const [token, setToken] = useState<string>(() => localStorage.getItem("taskly_token") || "");

  if (!token) {
    return <Login onAuthed={(t) => setToken(t)} />;
  }
  return <Tasks token={token} onLogout={() => setToken("")} />;
}
