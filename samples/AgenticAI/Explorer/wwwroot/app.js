const state = {
  samples: [],
  selected: null,
  mode: 'mock',
  runs: [],
  running: false
};

const elements = {
  sampleList: document.querySelector('#scenario-list'),
  sampleCount: document.querySelector('#sample-count'),
  sampleId: document.querySelector('#scenario-id'),
  title: document.querySelector('#scenario-title'),
  description: document.querySelector('#scenario-description'),
  goal: document.querySelector('#scenario-goal'),
  toolList: document.querySelector('#tool-list'),
  toolCount: document.querySelector('#tool-count'),
  status: document.querySelector('#ollama-status'),
  runButton: document.querySelector('#run-button'),
  turnLimit: document.querySelector('#turn-limit'),
  error: document.querySelector('#error-banner'),
  emptyResult: document.querySelector('#empty-result'),
  latestResult: document.querySelector('#latest-result'),
  latestValidity: document.querySelector('#latest-validity'),
  history: document.querySelector('#run-history'),
  clearHistory: document.querySelector('#clear-history')
};

async function loadWorkspace() {
  try {
    const [samplesResponse, statusResponse] = await Promise.all([
      fetch('/api/samples'),
      fetch('/api/status')
    ]);
    if (!samplesResponse.ok || !statusResponse.ok) throw new Error('Explorer API is unavailable.');

    state.samples = await samplesResponse.json();
    state.status = await statusResponse.json();
    elements.sampleCount.textContent = String(state.samples.length).padStart(2, '0');
    renderStatus();
    renderNavigation();
    selectSample(state.samples[0]);
  } catch (error) {
    showError(error.message);
    elements.title.textContent = 'Explorer unavailable';
    elements.description.textContent = 'The sample API could not be reached.';
  }
}

function renderStatus() {
  const online = state.status?.ollamaAvailable;
  elements.status.dataset.state = online ? 'online' : 'offline';
  elements.status.replaceChildren();
  const dot = document.createElement('span');
  dot.className = 'status-dot';
  const text = document.createElement('span');
  text.textContent = online
    ? `Ollama ready · ${state.status.model}`
    : 'Ollama not detected';
  elements.status.append(dot, text);
}

function renderNavigation() {
  elements.sampleList.replaceChildren();
  for (const sample of state.samples) {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'scenario-button';
    button.setAttribute('aria-current', String(sample.id === state.selected?.id));
    const number = document.createElement('span');
    number.className = 'scenario-number';
    number.textContent = sample.id;
    const name = document.createElement('span');
    name.className = 'scenario-name';
    name.textContent = sample.name;
    button.append(number, name);
    button.addEventListener('click', () => selectSample(sample));
    elements.sampleList.append(button);
  }
}

function selectSample(sample) {
  state.selected = sample;
  elements.sampleId.textContent = sample.id;
  elements.title.textContent = sample.name;
  elements.description.textContent = sample.description;
  elements.goal.textContent = sample.goal;
  elements.toolCount.textContent = String(sample.tools.length).padStart(2, '0');
  elements.toolList.replaceChildren();
  for (const toolName of sample.tools) {
    const item = document.createElement('li');
    const glyph = document.createElement('span');
    glyph.className = 'tool-glyph';
    glyph.setAttribute('aria-hidden', 'true');
    glyph.textContent = '↳';
    const label = document.createElement('span');
    label.textContent = toolName;
    item.append(glyph, label);
    elements.toolList.append(item);
  }
  renderNavigation();
  renderLatestRun();
  hideError();
}

function setMode(mode) {
  state.mode = mode;
}

async function runScenario() {
  if (state.running || !state.selected) return;
  state.running = true;
  elements.runButton.disabled = true;
  elements.runButton.firstElementChild.textContent = 'Running';
  hideError();
  const started = performance.now();

  try {
    const response = await fetch('/api/run', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        sampleId: state.selected.id,
        mode: state.mode,
        maxTurns: Number(elements.turnLimit.value)
      })
    });
    const result = await response.json();
    if (!response.ok) throw new Error(result.error ?? 'The agent run failed.');

    result.clientDurationMilliseconds = Math.round(performance.now() - started);
    result.runAt = new Date().toISOString();
    state.runs.unshift(result);
    state.runs = state.runs.slice(0, 8);
    renderLatestRun();
    renderHistory();
  } catch (error) {
    showError(error.message);
  } finally {
    state.running = false;
    elements.runButton.disabled = false;
    elements.runButton.firstElementChild.textContent = 'Run scenario';
  }
}

function renderLatestRun() {
  const run = state.runs.find(item => item.sampleId === state.selected?.id);
  elements.emptyResult.hidden = Boolean(run);
  elements.latestResult.hidden = !run;
  elements.latestResult.replaceChildren();
  if (!run) {
    elements.latestValidity.textContent = 'AWAITING RUN';
    delete elements.latestValidity.dataset.valid;
    return;
  }

  elements.latestValidity.textContent = run.valid ? 'VALIDATED' : 'CHECK REQUIRED';
  elements.latestValidity.dataset.valid = String(run.valid);
  const root = document.createElement('div');
  root.className = 'latest-run';

  const meta = document.createElement('div');
  meta.className = 'run-meta';
  meta.append(
    makeChip(run.mode === 'mock' ? 'MOCK CLIENT' : `OLLAMA · ${run.model}`, run.mode === 'ollama' ? 'mode-ollama' : ''),
    makeChip(`${run.turns}/${run.maxTurns} TURNS`),
    makeChip(`${run.durationMilliseconds} MS`)
  );

  const callsLabel = document.createElement('div');
  callsLabel.className = 'answer-label';
  callsLabel.textContent = `TOOL TRACE · ${run.toolCalls.length}`;
  const calls = document.createElement('p');
  calls.className = 'answer-text';
  calls.textContent = run.toolCalls.length ? run.toolCalls.join(' → ') : 'No tools called';

  const answerLabel = document.createElement('div');
  answerLabel.className = 'answer-label';
  answerLabel.textContent = 'AGENT RESPONSE';
  const answer = document.createElement('p');
  answer.className = 'answer-text';
  answer.textContent = run.finalAnswer;

  const checks = document.createElement('ul');
  checks.className = 'checks-list';
  for (const check of run.checks) {
    const row = document.createElement('li');
    row.className = 'check-row';
    const result = document.createElement('span');
    result.className = `check-result ${check.passed ? 'pass' : 'fail'}`;
    result.textContent = check.passed ? 'PASS' : 'REVIEW';
    const detail = document.createElement('span');
    detail.className = 'check-detail';
    detail.textContent = `${check.name}: ${check.detail}`;
    row.append(result, detail);
    checks.append(row);
  }

  root.append(meta, callsLabel, calls, answerLabel, answer, checks);
  elements.latestResult.append(root);
}

function makeChip(text, extraClass = '') {
  const chip = document.createElement('span');
  chip.className = `meta-chip ${extraClass}`.trim();
  chip.textContent = text;
  return chip;
}

function renderHistory() {
  elements.history.replaceChildren();
  elements.clearHistory.disabled = state.runs.length === 0;
  if (state.runs.length === 0) {
    const empty = document.createElement('p');
    empty.className = 'history-empty';
    empty.textContent = 'Runs from this session appear here.';
    elements.history.append(empty);
    return;
  }

  for (const run of state.runs) {
    const scenario = state.samples.find(sample => sample.id === run.sampleId);
    const entry = document.createElement('article');
    entry.className = 'history-entry';
    const time = document.createElement('time');
    time.className = 'history-time';
    const runDate = new Date(run.runAt);
    time.dateTime = run.runAt;
    time.textContent = `${runDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} · ${run.mode.toUpperCase()}`;
    const summary = document.createElement('div');
    summary.className = 'history-summary';
    const title = document.createElement('strong');
    title.textContent = scenario?.name ?? run.sampleName;
    const excerpt = document.createElement('span');
    excerpt.textContent = run.finalAnswer;
    summary.append(title, excerpt);
    const stateLabel = document.createElement('span');
    stateLabel.className = `history-state ${run.valid ? '' : 'fail'}`;
    stateLabel.textContent = run.valid ? 'VALIDATED' : 'REVIEW';
    entry.append(time, summary, stateLabel);
    elements.history.append(entry);
  }
}

function showError(message) {
  elements.error.textContent = message;
  elements.error.hidden = false;
}

function hideError() {
  elements.error.hidden = true;
  elements.error.textContent = '';
}

document.querySelectorAll('input[name="mode"]').forEach(input => {
  input.addEventListener('change', event => setMode(event.target.value));
});
elements.runButton.addEventListener('click', runScenario);
elements.clearHistory.addEventListener('click', () => {
  state.runs = [];
  renderHistory();
  renderLatestRun();
});

await loadWorkspace();
globalThis.setInterval(async () => {
  try {
    const response = await fetch('/api/status');
    if (response.ok) {
      state.status = await response.json();
      renderStatus();
    }
  } catch {
    elements.status.dataset.state = 'offline';
    elements.status.lastElementChild.textContent = 'Status unavailable';
  }
}, 15000);