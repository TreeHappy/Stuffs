## Inference Graph Structures

### Classical Confounding
- **Description:** A third variable influences both the exposure and outcome.
- **Implication:** Can bias the estimated effect of exposure on outcome.
- **Mermaid Diagram:**
  ```mermaid
  graph TD
  A[Exposure] --> B[Outcome]
  C[Confounder] --> A
  C --> B
  ```

---

### Collapsing Confounders
- **Description:** Controlling for a confounder alters the association between exposure and outcome.
- **Implication:** Alters the apparent strength of the exposure-outcome relationship.
- **Mermaid Diagram:**
  ```mermaid
  graph TD
  A[Exposure] --> B[Outcome]
  C[Confounder] --> A
  C --> B
  subgraph Collapsing
  C --> D[Controlled]
  A --> E[Adjusted Outcome]
  end
  ```

---

### Interaction Confounding
- **Description:** The effect of the exposure on the outcome varies across levels of the confounder.
- **Implication:** Suggests the need for stratified analysis based on confounder levels.
- **Mermaid Diagram:**
  ```mermaid
  graph TD
  A[Exposure] --> B[Outcome]
  C[Confounder 1] --> B
  C --> D[Confounder 2]
  A --> D
  ```

---

### Mediator
- **Description:** A variable that is on the causal pathway between the exposure and outcome.
- **Implication:** Necessary for understanding the mechanism of the exposure's effect.
- **Mermaid Diagram:**
  ```mermaid
  graph TD
  A[Exposure] --> B[Mediator] --> C[Outcome]
  ```

---

### Confounder
- **Description:** A variable that distorts the true relationship between exposure and outcome.
- **Implication:** Requires adjustment in statistical analyses to clarify the effect.
- **Mermaid Diagram:**
  ```mermaid
  graph TD
  A[Exposure] --> B[Outcome]
  C[Confounder] --> A
  C --> B
  ```

---

### Colluders
- **Description:** Variables affected by both the exposure and outcome but do not directly influence each other.
- **Implication:** Can complicate causal inference by appearing as confounders.
- **Mermaid Diagram:**
  ```mermaid
  graph TD
  A[Exposure] --> C1[Colluder] --> B[Outcome]
  A --> C2[Colluder] --> B
  ```

---

### Common Causes
- **Description:** Variables that influence both the exposure and outcome, leading to spurious associations.
- **Implication:** Important to identify and control for these in analyses.
- **Mermaid Diagram:**
  ```mermaid
  graph TD
  C[Common Cause] --> A[Exposure]
  C --> B[Outcome]
  ```

---

### Chain of Causation
- **Description:** A series of variables where one variable influences another in a sequential manner.
- **Implication:** Helps to understand how effects propagate through a system.
- **Mermaid Diagram:**
  ```mermaid
  graph TD
  A[Variable 1] --> B[Variable 2] --> C[Variable 3] --> D[Outcome]
  ```

---

### Path Analysis
- **Description:** A set of relationships modeled to show the direct and indirect effects among variables.
- **Implication:** Provides insight into complex relationships among multiple variables.
- **Mermaid Diagram:**
  ```mermaid
  graph TD
  A[Variable 1] -->|Effect 1| B[Variable 2]
  B -->|Effect 2| C[Variable 3]
  A -->|Effect 3| C
  ```

---

## Additional Concepts for Proper Experimental Design

- **Randomization:** Control for confounding variables by randomly assigning subjects to different treatment groups.
  
- **Control Groups:** Use a control group as a baseline to compare the effects of the intervention.

- **Blinding:** Implement single or double blinding to reduce bias in reported outcomes.

- **Sample Size Calculation:** Ensure adequate sample size to detect a meaningful effect while minimizing Type I and Type II errors.

- **Longitudinal Studies:** Conduct studies over time to observe trends and changes in outcomes.

- **Cross-Sectional Studies:** Observe a population at a single point in time to identify associations.

- **Statistical Analysis:** Utilize appropriate statistical methods to analyze data and draw valid conclusions.

Feel free to reach out if you need further elaboration on any of these concepts!

