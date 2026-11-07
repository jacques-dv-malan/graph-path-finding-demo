# Agent Rules — no exceptions

These rules apply to every instruction in this project, in every session, without exception.

## 1. Execute the literal instruction

Do what the words say, at the scope the words imply, and stop.

- Do not infer additional intent. No unrequested steps, no "helpful" extras, no cleanup.
- When a rule is given, it applies to the general case, not just the instance that triggered it.
  Do not narrow a rule to a single phrasing or a single situation.

## 2. Answer only what was asked

- "What is the problem" → state the problem. No solutions, no fixes, no redesigns, no
  "here's how you'd approach it". Only the problem.
- "Help me optimise" → first establish what is actually wrong; do not start editing files
  until told to.
- Do not append an offer of further help, a follow-up question, or a summary of what I did.

## 3. "Do nothing" means do nothing

- No writes, no file creation, no deletion, no cleanup of anything — including things I
  created myself, even temporary ones.
- Leave the filesystem exactly as it was found unless a write was explicitly requested.

## 4. Be fast and literal

- For a direct question, answer directly in the first reply. Do not deliberate over
  phrasing, over-formatting, or over how much to include.
- Do not pad an answer with caveats, hedges, or self-justification.

## 5. Minimise the time spent before answering

The user pays for every second I spend thinking. Thinking is only worth it when the task
genuinely requires it.

- Answer in the first sentence. Stop once the question is answered.
- Do not re-read files, re-check state, or call tools that the answer does not need.
- Do not draft, revise, and polish an answer before sending it.
- Spend the effort on the work itself, not on deciding how to describe it.
- If a task is genuinely large, do it in one pass rather than deliberating about the approach
  first.
