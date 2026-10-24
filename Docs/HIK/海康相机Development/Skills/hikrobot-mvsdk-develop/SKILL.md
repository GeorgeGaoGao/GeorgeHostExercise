---
name: hikrobot-mvsdk-develop
description: >-
  Use this skill when the user writes, debugs, reviews, or explains code that
  calls the MVS SDK — Hikrobot's SDK for industrial cameras and frame grabbers —
  in C/C++, .NET (C#), or Python, even when the SDK is not named. Mentions of
  MvCameraControl, MV_CC_* APIs, or MvCamCtrl are strong signals. Trigger for
  symptoms (device open failure, dropped frames, grab timeouts, callbacks never
  firing, error-code returns), tasks (enumerate cameras, grab frames, set
  trigger mode, exposure, gain, or other camera parameters, convert pixel
  formats), help writing correct code or diagnosing API misuse, and reference
  questions (APIs, structs, error codes, header/lib locations, official
  samples). Hikrobot MVS SDK only — not other vendors' camera SDKs, not the
  MVS client GUI, not general machine-vision questions unrelated to
  development.
metadata:
  version: "1.0.0"
compatibility: >-
  Requires the Hikrobot MVS SDK installed on the machine. Covers C/C++,
  .NET (C#, both MvCamCtrl.NET V1 and MvCameraControl V2), and Python on Windows.
---

# MVS SDK Programmer's Guide

This document is the entry point for the skill. The reference documents live in the
`references/` subdirectory (alongside this SKILL.md file) and cover the C/C++, C#, and Python APIs.

## Reference documents

This skill contains the following four documents:

1. **`references/sdk-directory-reference.md`** — **Read this first, before any task.** It is the
   index to the whole SDK: what lives where (SDK paths), the API definition file locations per
   language, a searchable API keyword catalog covering every capability category, and the
   per-language differences (V1 vs V2, the V2 object model).

2. **`references/sdk-usage-notes.md`** — Development hard rules, API calling conventions, API
   precautions, common troubleshooting.
   **Development conventions + pitfall-avoidance guide**, used to answer "how should this be
   written" and "how do I avoid this pitfall".

3. **`references/sdk-error-codes.md`** — Error codes (`MV_*` macros) grouped by module, each with
   its cause and troubleshooting steps. **Never read this document whole — always search it for
   the one error code you got**, then read only that entry. When to use it: an SDK API call
   returned a failure code (anything other than `MV_OK`) and you need the specific reason and how
   to resolve it. How to use it: every entry is a heading of the form `### <macro> (<hex value>)`,
   so grep for either form and read the few lines that follow —
   `grep -A 12 "0x80000003" sdk-error-codes.md` or `grep -A 12 "MV_E_CALLORDER" sdk-error-codes.md`.

4. **`references/sdk-samples-reference.md`** — Per-language inventory of the SDK's sample
   projects (C++/C#/Python/other). **Read on demand** when generating example code or looking
   for a reference project — not part of the mandatory first read.

## How to use

1. **What exists / where is it** — SDK paths, which capabilities the SDK has, where the API is
   defined → `references/sdk-directory-reference.md`
   (**mandatory first read**).
2. **How exactly is it defined** — the precise signature, struct field, enum constant or macro
   value → the SDK's own definitions: the per-language definition files listed in
   "Where the APIs are defined" inside `references/sdk-directory-reference.md`.
3. **API calling conventions / pitfalls / common issues** → read
   `references/sdk-usage-notes.md`
4. **An API call failed** — the returned error code, its cause and how to resolve it →
   `references/sdk-error-codes.md`. **Never read this document whole**: grep it for the single
   code you got (`0x80000003` or `MV_E_CALLORDER`) and read only that entry's few lines. The file
   is ~1700 lines; reading it in full wastes context.
5. **Code samples** → the sample inventory in `references/sdk-samples-reference.md`; prefer the
   code templates in the usage notes, and look up API details in the definition files listed in
   `references/sdk-directory-reference.md`
