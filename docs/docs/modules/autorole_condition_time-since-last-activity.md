﻿Autorole Condition Time Since Last Activity Commands
====================================================
## Summary
These commands are prefixed with `autorole condition time-since-last-activity`. You can also use `at condition time-since-last-activity` instead of `autorole condition time-since-last-activity`.

## Commands
### *autorole condition time-since-last-activity*
#### Overloads
**`!autorole condition time-since-last-activity "placeholder" 5m`**

Adds an instance of the condition to the role.

| Name | Type | Optional |
| --- | --- | --- |
| autorole | AutoroleConfiguration | `no` |
| time | TimeSpan | `no` |

**`!autorole condition time-since-last-activity "placeholder" 10 5m`**

Modifies an instance of the condition on the role.

| Name | Type | Optional |
| --- | --- | --- |
| autorole | AutoroleConfiguration | `no` |
| conditionID | long | `no` |
| time | TimeSpan | `no` |

<sub><sup>Generated by DIGOS.Ambassador.Doc</sup></sub>