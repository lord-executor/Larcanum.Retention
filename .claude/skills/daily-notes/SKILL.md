---
name: daily-notes
description: Write a session summary/log file under doc/sessions/. Use at the end of a session, or when the user asks to log, record, or write up what was done ("write session notes", "log this session", "save a summary of today's work").
---

Summarizes the current session's work into a short Markdown file in `doc/sessions/`, named after the timestamp the session started (not "today's date" alone — sessions are per-file, so the filename must be precise enough to avoid collisions between multiple sessions on the same day).

## 1. Determine the session start time

Claude Code stores this conversation's transcript as a `.jsonl` file under:

```
~/.claude/projects/<slug>/<session-id>.jsonl
```

where `<slug>` is the repository's absolute path with every non-alphanumeric character replaced by `-` (e.g. `D:\storage\development\projects\Larcanum.Retention` becomes `D--storage-development-projects-Larcanum-Retention`).

To find the right file and its start time:

1. List `*.jsonl` files in that project directory.
2. If there's only one modified "recently" (this session), use it. If there are several (parallel sessions), disambiguate by grepping for a distinctive snippet of *this conversation's actual first user message* — the file containing it is this session's transcript.
3. Read that file's first line where `"type":"user"` appears. It has a top-level `"timestamp"` field, e.g. `"timestamp":"2026-08-08T13:32:09.128Z"` — ISO-8601 UTC. Convert to local time; that is the session start time.

Example PowerShell to locate the directory and list candidates:

```powershell
$slug = ($pwd.Path -replace '[^a-zA-Z0-9]', '-')
$dir = Join-Path $env:USERPROFILE ".claude\projects\$slug"
Get-ChildItem $dir -Filter *.jsonl | Sort-Object LastWriteTime -Descending
```

Then read the candidate file's first `"type":"user"` line to pull the `timestamp` field.

## 2. Build the filename

Format the local session-start time as `yyyy-MM-dd_HH-mm-ss.md` (sortable, filesystem-safe). Create `doc/sessions/` if it doesn't already exist.

## 3. Write the summary

Keep it factual and concise — a work log, not a narrative. Structure:

```markdown
# Session Notes — <yyyy-MM-dd HH:mm> (local)

## Summary
One or two sentences on what this session was about.

## Completed
- Concrete change or task, with file references where useful (`path/to/file.cs`).
- ...

## Follow-ups / Pending
- Anything left open or intentionally deferred (omit this section entirely if there's nothing pending).
```

Base the content on what actually happened in this conversation — real files touched, decisions made, problems solved — not a generic restatement of the user's request.

## 4. Save and report

Write the file to `doc/sessions/<filename>.md` and tell the user the path. Do not stage or commit it — per this repo's rule, committing is the user's job unless they explicitly ask.
