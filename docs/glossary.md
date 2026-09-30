# 📖 Comprehensive Agentic AI & LLM Glossary

A quick-reference guide to essential terminology in Generative AI, Large Language Models (LLMs), and Agentic Systems.

---

### A
- **Agent**: An autonomous computational entity that perceives its environment, makes decisions, invokes tools, and takes actions to accomplish goals.
- **Agentic AI**: Systems where LLMs plan, reason, invoke functions, observe outcomes, and self-correct across multi-step execution loops.
- **AIFunction**: In `Microsoft.Extensions.AI`, a strongly typed abstraction representing a callable C# function or API tool that an LLM can invoke.
- **Autoregressive Generation**: The generation mechanism where a model produces text token-by-token, using previous tokens as context for the next token.

---

### C
- **Chain-of-Thought (CoT)**: A prompt engineering technique that instructs the LLM to break complex reasoning into sequential intermediate steps before answering.
- **Chunking**: The process of splitting large documents into smaller, semantically coherent segments suitable for vector embedding and context windows.
- **Context Window**: The maximum number of tokens an LLM can process in a single request (including system prompt, history, tools, and output).
- **Cosine Similarity**: A mathematical metric measuring the cosine of the angle between two multi-dimensional vectors, used to assess semantic similarity.

---

### D
- **Dense Vector**: An array of floating-point numbers where most values are non-zero, encoding dense semantic representations of words, sentences, or documents.
- **Deterministic Output**: Generating predictable, identical responses across runs, typically achieved with Temperature = 0.0 and fixed random seeds.

---

### E
- **Embedding**: A numerical vector representation of text in a continuous vector space where semantically similar texts are located close to each other.
- **Evaluation / LLM-as-a-Judge**: Using a high-capability LLM to systematically evaluate candidate outputs against criteria like Faithfulness, Relevance, and Coherence.

---

### F
- **Faithfulness / Groundedness**: The degree to which an AI-generated answer is strictly supported by provided reference facts without hallucinated claims.
- **Few-Shot Prompting**: Providing one or more example input-output pairs within the prompt to demonstrate expected format and reasoning.
- **Function Calling (Tool Use)**: The capability of an LLM to recognize when it needs external information or actions and output structured parameters to call specific software functions.

---

### G
- **Guardrails**: Safety filters, regex rules, and semantic classifiers applied before (pre-guardrail) and after (post-guardrail) LLM execution to block PII, toxic content, and prompt injections.

---

### H
- **Hallucination**: When an LLM generates plausible-sounding but factually incorrect or ungrounded assertions.

---

### I
- **IChatClient**: The foundational `Microsoft.Extensions.AI` interface for chat completions, streaming responses, and tool calling across any AI provider.
- **IEmbeddingGenerator**: The foundational `Microsoft.Extensions.AI` interface for converting strings into vector embeddings.

---

### M
- **Multi-Agent Orchestration**: Coordinating multiple specialized agent personas (e.g., Architect, Coder, Auditor) to solve complex workflows collaboratively.

---

### P
- **Persona / System Prompt**: Foundational prompt instructions that establish the AI agent's role, behavioral boundaries, expertise, and communication tone.
- **Plan-and-Solve**: An agentic pattern that separates problem-solving into a strategic planning phase (goal decomposition) and a sequential execution phase.
- **Prompt Injection**: An adversarial attack where malicious user input manipulates the LLM into ignoring previous system instructions or safety rules.

---

### R
- **RAG (Retrieval-Augmented Generation)**: An architecture that enriches user prompts with relevant factual excerpts retrieved from an external vector database or search engine.
- **ReAct (Reasoning + Acting)**: An agentic loop pattern interleaving *Thought* (reasoning), *Action* (tool execution), and *Observation* (environment feedback).

---

### S
- **Semantic Search**: Searching documents based on conceptual meaning rather than exact keyword matches, powered by vector embeddings.
- **Sliding Window Buffer**: A memory management pattern that retains the most recent $K$ conversation turns to fit within context limits.
- **Structured Output**: Forcing the LLM to output valid JSON matching a defined schema (e.g., C# class) rather than freeform text.

---

### T
- **Temperature**: A hyperparameter controlling randomness in token generation. Lower values (0.0-0.2) produce deterministic outputs; higher values (0.7-1.0) increase creativity.
- **Token**: The basic unit of text processed by an LLM (typically sub-word fragments, where 1 token $\approx$ 4 English characters).
- **Top-K**: The number of highest-scoring nearest neighbors retrieved during vector similarity search.

---

### Z
- **Zero-Shot Prompting**: Asking the model to perform a task directly without providing any prior examples in the prompt.
