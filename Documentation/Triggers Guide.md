## Triggers Guide

### Triggers Out

- **MI** (Motor Imagery)
  - 30 left, 30 right per phase
  - Left trigger: `1`
  - Right trigger: `2`

- **MVEP** (Motion Visual Evoked Potential)
  - ~60 trials?
  - Trigger 1: `11`
  - Trigger 2: `12`
  - Trigger 3: `13`
  - Trigger 4: `14`
  - Trigger 5: `15`

- **Neurofeedback**
  - (Are there settings for a set number of trials atm?)
  - Trigger start: `21`
  - Trigger end: `22`

- **Neurofeedback 2** (Passive)
  - Only need enter/exit?
  - Trigger 1 start: `31`
  - Trigger 2 start: `32`
  - Trigger 3 start: `33`
  - Trigger 4 start: `34`
  - Trigger end: `39`

---

### Enter / Exit scenes
*Early warning that a specific game will be starting soon, to allow loading the correct MATLAB code*

- MI: `81` / `91`
- MVEP: `82` / `92`
- NF: `83` / `93`
- NF2: `84` / `94`

---

### Triggers In (from MATLAB or UDP Simulator)

- **MI**
  - Float `[-1, 1]`

- **MVEP**
  - Int `1`, `2`, `3`, `4`, `5`
  - Or potentially `11`, `12`, `13`, `14`, `15`

- **NF**
  - Float `0` to `1`
  - Potentially using the same feedback as NF2, selecting a specific element

- **NF2**
  - Float array
  - Example: `[0..1, 0..1, 0..1, 0..1, 0..1, 0..1]`
  - Number of array elements: TBD
