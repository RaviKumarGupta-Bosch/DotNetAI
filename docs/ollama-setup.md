# 🦙 Ollama Local Setup & Configuration Guide

This guide provides step-by-step instructions for installing, configuring, and testing local LLMs with Ollama for the DotNetAI project.

---

## 1. What is Ollama?

[Ollama](https://ollama.com/) is a lightweight, open-source tool that allows you to run large language models (LLMs) locally on Windows, macOS, and Linux. It wraps `llama.cpp` in a streamlined background service with a local REST API listening on `http://localhost:11434`.

---

## 2. Installation on Windows

1. Download the Windows installer from [https://ollama.com/download/windows](https://ollama.com/download/windows).
2. Run `OllamaSetup.exe` and complete the installation wizard.
3. Ollama will start automatically and sit in your Windows system tray.
4. Verify the installation in PowerShell:
   ```powershell
   ollama --version
   ```

---

## 3. Pulling Recommended Models

For the DotNetAI use cases, we recommend pulling the following models:

### 3.1 Primary Chat Model (LLaMA 3.2 3B)
```powershell
ollama pull llama3.2
```
*Compact (approx 2.0 GB), fast inference, excellent reasoning and tool-calling capabilities.*

### 3.2 Alternative Fast Model (Phi-3.5 Mini 3.8B)
```powershell
ollama pull phi3.5
```
*Microsoft's state-of-the-art small language model with high reasoning density.*

### 3.3 Text Embedding Model (Nomic Embed Text)
```powershell
ollama pull nomic-embed-text
```
*High-performance 768-dimensional embedding model tailored for RAG and semantic search.*

---

## 4. Verifying Ollama Health

Test that the Ollama REST API is responding:

```powershell
curl http://localhost:11434/api/tags
```

You should see a JSON response listing your installed models:
```json
{
  "models": [
    {
      "name": "llama3.2:latest",
      "model": "llama3.2:latest",
      "modified_at": "2025-01-15T10:00:00Z",
      "size": 2019393189,
      "digest": "a80c4f17..."
    }
  ]
}
```

---

## 5. Environment Variables & Port Configuration

By default, DotNetAI connects to `http://localhost:11434` with model `llama3.2` and embedding model `nomic-embed-text`.

You can customize these via environment variables or in `appsettings.json`:

```json
{
  "AI": {
    "Provider": "Ollama",
    "OllamaEndpoint": "http://localhost:11434",
    "ModelName": "llama3.2",
    "EmbeddingModelName": "nomic-embed-text",
    "Temperature": 0.2
  }
}
```

Or in PowerShell:
```powershell
$env:AI__Provider = "Ollama"
$env:AI__ModelName = "llama3.2"
$env:AI__OllamaEndpoint = "http://localhost:11434"
```

---

## 6. Running DotNetAI with Ollama

Run any use case or the interactive runner with Ollama:

```powershell
# Run the interactive runner (will auto-detect Ollama)
dotnet run --project src/DotNetAI.Runner

# Run specific use cases
dotnet run --project src/01-PromptEngineering
dotnet run --project src/04-RAG-VectorSearch
dotnet run --project src/05-Agentic-ReAct
```

---

## 7. Troubleshooting

| Issue | Cause | Resolution |
| :--- | :--- | :--- |
| **Connection refused on localhost:11434** | Ollama service is stopped | Launch Ollama from the Windows Start menu or run `ollama serve`. |
| **Model not found error** | Model hasn't been pulled | Run `ollama pull llama3.2`. |
| **Out of Memory (OOM) / Slow Inference** | Insufficient GPU VRAM | LLaMA 3.2 3B requires ~3-4 GB VRAM or system RAM. For low-spec machines, use `phi3:mini` or mock mode (`--mock`). |
| **DotNetAI falls back to Mock mode** | Ollama endpoint unreachable | Ensure Ollama is running, then pass `--ollama` explicitly. |
