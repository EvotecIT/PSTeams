---
title: "MessageX overview"
description: "Teams Workflow, Slack, and Discord delivery from one MessageX module."
layout: docs
---

MessageX provides typed PowerShell commands and .NET libraries for Teams, Slack, and Discord notifications. Teams delivery uses a send-only Workflow webhook. Office 365 connector URLs were retired in May 2026.

The product-facing notification data has severity, title, short explanation, facts, incident ID, report URL, and a plain-text fallback. Each provider keeps its native message format. For Teams, an Adaptive Card can open a report with `Action.OpenUrl`; Workflow URLs cannot upload a channel file, reply to a message, or process submit actions.

The module is currently an unpublished candidate. Start with [local installation](../install/) and the [self-contained examples](/projects/psteams/examples/). Keep Workflow URLs and provider tokens outside source control.
