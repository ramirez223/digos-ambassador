﻿Warning Set Commands
====================
## Summary
These commands are prefixed with `warning set`. You can also use `warn set` instead of `warning set`.

## Commands
### *reason*
#### Overloads
**`warning set reason`**

Sets the reason for the warning.

| Name | Type | Optional |
| --- | --- | --- |
| warningID | long | `no` |
| newReason | string | `no` |

---

### *context-message*
#### Overloads
**`warning set context-message`**

Sets the contextually relevant message for the warning.

| Name | Type | Optional |
| --- | --- | --- |
| warningID | long | `no` |
| newMessage | IMessage | `no` |

---

### *duration*
#### Overloads
**`warning set duration`**

Sets the duration of the warning.

| Name | Type | Optional |
| --- | --- | --- |
| warningID | long | `no` |
| newDuration | TimeSpan | `no` |

<sub><sup>Generated by DIGOS.Ambassador.Doc</sup></sub>