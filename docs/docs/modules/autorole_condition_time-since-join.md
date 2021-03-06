﻿Autorole Condition Time Since Join Commands
===========================================
## Summary
These commands are prefixed with `autorole condition time-since-join`. You can also use `at condition time-since-join` instead of `autorole condition time-since-join`.

## Commands
### *autorole condition time-since-join*
#### Overloads
**`!autorole condition time-since-join @Users 5m`**

Adds an instance of the condition to the role.

| Name | Type | Optional |
| --- | --- | --- |
| autorole | AutoroleConfiguration | `no` |
| time | TimeSpan | `no` |

**`!autorole condition time-since-join @Users 10 5m`**

Modifies an instance of the condition on the role.

| Name | Type | Optional |
| --- | --- | --- |
| autorole | AutoroleConfiguration | `no` |
| conditionID | long | `no` |
| time | TimeSpan | `no` |

<sub><sup>Generated by DIGOS.Ambassador.Doc</sup></sub>