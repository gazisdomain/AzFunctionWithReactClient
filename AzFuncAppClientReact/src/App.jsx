import { useEffect, useState } from "react";

const API_BASE = import.meta.env.VITE_API_BASE; // from your .env

export default function App() {
    const [todos, setTodos] = useState([]);
    const [title, setTitle] = useState("");
    const [busyId, setBusyId] = useState(null); // track which row is busy
    const [error, setError] = useState("");

    async function loadTodos() {
        setError("");
        const res = await fetch(`${API_BASE}/todos`);
        if (!res.ok) { setError(`GET /todos failed: ${res.status}`); return; }
        setTodos(await res.json());
    }

    async function addTodo() {
        if (!title.trim()) return;
        setError("");
        const res = await fetch(`${API_BASE}/todos`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ title })
        });
        if (!res.ok) { setError(`POST /todos failed: ${res.status}`); return; }
        setTitle("");
        await loadTodos();
    }

    async function delTodo(id) {
        if (!confirm("Delete this todo?")) return;
        setBusyId(id); setError("");
        try {
            const res = await fetch(`${API_BASE}/todos/${id}`, { method: "DELETE" });
            if (res.status !== 204 && res.status !== 200) {
                throw new Error(`DELETE failed: ${res.status}`);
            }
            // Optimistic update (optional): remove locally without reloading
            setTodos(prev => prev.filter(t => t.id !== id));
        } catch (e) {
            setError(e.message);
            await loadTodos(); // fallback refresh
        } finally {
            setBusyId(null);
        }
    }

    useEffect(() => { loadTodos(); }, []);

    return (
        <main style={{ fontFamily: "system-ui", padding: 24, maxWidth: 720, margin: "0 auto" }}>
            <h1>Todos (React → Azure Functions)</h1>

            <div style={{ display: "flex", gap: 8, marginBottom: 12 }}>
                <input
                    value={title}
                    onChange={e => setTitle(e.target.value)}
                    placeholder="New todo title"
                    style={{ flex: 1, padding: 8, borderRadius: 8, border: "1px solid #ddd" }}
                />
                <button onClick={addTodo}>Add</button>
            </div>

            {error && <div style={{ color: "crimson", marginBottom: 12 }}>⚠️ {error}</div>}

            <ul style={{ listStyle: "none", padding: 0, display: "grid", gap: 8 }}>
                {todos.map(t => (
                    <li key={t.id} style={{ padding: 12, border: "1px solid #eee", borderRadius: 12, display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                        <div>
                            <div style={{ fontWeight: 600 }}>{t.title}</div>
                            <div style={{ fontSize: 12, color: "#666" }}>{t.id}</div>
                            <div style={{ fontSize: 12, color: t.isDone ? "green" : "#555" }}>done: {String(t.isDone)}</div>
                        </div>
                        <div style={{ display: "flex", gap: 8 }}>
                            <button
                                onClick={() => delTodo(t.id)}
                                disabled={busyId === t.id}
                                title="Delete"
                            >
                                {busyId === t.id ? "Deleting..." : "Delete"}
                            </button>
                        </div>
                    </li>
                ))}
            </ul>
        </main>
    );
}
