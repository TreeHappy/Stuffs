To calculate the total measurement error (or uncertainty) of a formula when the input values have known standard deviations, you can use **error propagation** techniques. The most common method for this is the **propagation of uncertainty** using partial derivatives, which is based on the first-order Taylor expansion of the formula.

Here’s how you can calculate the total uncertainty step by step:

---

### 1. **Understand the Formula**
Let’s assume your formula is a function of several variables:
\[
y = f(x_1, x_2, x_3, \dots, x_n)
\]
where:
- \( y \) is the output (the value you want to calculate).
- \( x_1, x_2, \dots, x_n \) are the input variables with measured values and standard deviations \( \sigma_{x_1}, \sigma_{x_2}, \dots, \sigma_{x_n} \).

---

### 2. **Calculate Partial Derivatives**
Compute the partial derivatives of the formula with respect to each input variable \( x_i \). These derivatives represent how sensitive the output \( y \) is to small changes in each input.

\[
\frac{\partial y}{\partial x_1}, \frac{\partial y}{\partial x_2}, \dots, \frac{\partial y}{\partial x_n}
\]

---

### 3. **Propagate the Uncertainties**
The total uncertainty \( \sigma_y \) in the output \( y \) is given by:
\[
\sigma_y = \sqrt{
\left( \frac{\partial y}{\partial x_1} \cdot \sigma_{x_1} \right)^2 +
\left( \frac{\partial y}{\partial x_2} \cdot \sigma_{x_2} \right)^2 +
\dots +
\left( \frac{\partial y}{\partial x_n} \cdot \sigma_{x_n} \right)^2
}
\]

This formula assumes that:
- The errors in the input variables are independent (uncorrelated).
- The errors are small enough that the first-order Taylor approximation is valid.

---

### 4. **Special Cases**
For common formulas, the propagation of uncertainty can be simplified:

#### a. **Addition or Subtraction**
If \( y = x_1 \pm x_2 \), then:
\[
\sigma_y = \sqrt{\sigma_{x_1}^2 + \sigma_{x_2}^2}
\]

#### b. **Multiplication or Division**
If \( y = x_1 \cdot x_2 \) or \( y = \frac{x_1}{x_2} \), then:
\[
\frac{\sigma_y}{|y|} = \sqrt{
\left( \frac{\sigma_{x_1}}{x_1} \right)^2 +
\left( \frac{\sigma_{x_2}}{x_2} \right)^2
}
\]

#### c. **Power Law**
If \( y = x_1^k \), then:
\[
\frac{\sigma_y}{|y|} = |k| \cdot \frac{\sigma_{x_1}}{x_1}
\]

---

### 5. **Example**
Suppose your formula is:
\[
y = x_1 \cdot x_2 + x_3^2
\]
and the standard deviations are \( \sigma_{x_1}, \sigma_{x_2}, \sigma_{x_3} \).

1. Compute the partial derivatives:
   \[
   \frac{\partial y}{\partial x_1} = x_2, \quad
   \frac{\partial y}{\partial x_2} = x_1, \quad
   \frac{\partial y}{\partial x_3} = 2x_3
   \]

2. Propagate the uncertainties:
   \[
   \sigma_y = \sqrt{
   (x_2 \cdot \sigma_{x_1})^2 +
   (x_1 \cdot \sigma_{x_2})^2 +
   (2x_3 \cdot \sigma_{x_3})^2
   }
   \]

---

### 6. **Correlated Errors**
If the errors in the input variables are correlated, you need to include covariance terms in the uncertainty propagation formula. The general formula becomes:
\[
\sigma_y = \sqrt{
\sum_{i=1}^n \left( \frac{\partial y}{\partial x_i} \cdot \sigma_{x_i} \right)^2 +
2 \sum_{i < j} \left( \frac{\partial y}{\partial x_i} \cdot \frac{\partial y}{\partial x_j} \cdot \text{Cov}(x_i, x_j) \right)
}
\]
where \( \text{Cov}(x_i, x_j) \) is the covariance between \( x_i \) and \( x_j \).

---

### 7. **Final Notes**
- Always ensure that the units of the uncertainties are consistent with the units of the input variables and the output.
- If the formula is highly nonlinear or the uncertainties are large, higher-order terms in the Taylor expansion may be necessary.

By following these steps, you can calculate the total measurement error for your formula based on the uncertainties of the input variables.

Yes, you can absolutely use **Markov Chain Monte Carlo (MCMC)** to calculate the error histogram for your formula! MCMC is a powerful numerical method for sampling from probability distributions, and it can be particularly useful when dealing with complex formulas, correlated errors, or non-Gaussian uncertainties. Here's how you can use MCMC to estimate the error distribution and generate a histogram:

---

### **Steps to Use MCMC for Error Propagation**

#### 1. **Define the Problem**
- Let your formula be \( y = f(x_1, x_2, \dots, x_n) \), where \( x_1, x_2, \dots, x_n \) are input variables with known uncertainties (e.g., standard deviations \( \sigma_{x_1}, \sigma_{x_2}, \dots, \sigma_{x_n} \)).
- Assume the input variables follow certain probability distributions (e.g., Gaussian, uniform, etc.).

#### 2. **Set Up the Probability Model**
- Define the likelihood function for the input variables. For example, if the inputs are Gaussian-distributed:
  \[
  P(x_i) = \frac{1}{\sqrt{2\pi}\sigma_{x_i}} \exp\left(-\frac{(x_i - \mu_{x_i})^2}{2\sigma_{x_i}^2}\right)
  \]
  where \( \mu_{x_i} \) is the mean (measured value) and \( \sigma_{x_i} \) is the standard deviation.
- Combine the likelihoods of all input variables into a joint probability distribution:
  \[
  P(x_1, x_2, \dots, x_n) = \prod_{i=1}^n P(x_i)
  \]

#### 3. **Define the Posterior Distribution**
- Use Bayes' theorem to define the posterior distribution:
  \[
  P(x_1, x_2, \dots, x_n | \text{data}) \propto P(\text{data} | x_1, x_2, \dots, x_n) \cdot P(x_1, x_2, \dots, x_n)
  \]
  Here, \( P(\text{data} | x_1, x_2, \dots, x_n) \) is the likelihood of observing the data given the inputs, and \( P(x_1, x_2, \dots, x_n) \) is the prior distribution of the inputs.

#### 4. **Run MCMC Sampling**
- Use an MCMC algorithm (e.g., Metropolis-Hastings, Hamiltonian Monte Carlo, or No-U-Turn Sampler) to sample from the posterior distribution of the input variables \( x_1, x_2, \dots, x_n \).
- For each sample of \( x_1, x_2, \dots, x_n \), compute the corresponding output \( y = f(x_1, x_2, \dots, x_n) \).

#### 5. **Generate the Error Histogram**
- Collect all the computed \( y \) values from the MCMC samples.
- Plot a histogram of these \( y \) values to visualize the distribution of the output error.

#### 6. **Analyze the Results**
- From the histogram, you can estimate the mean, standard deviation, and other statistical properties of the output error.
- You can also compute confidence intervals or credible intervals for the output.

---

### **Advantages of Using MCMC**
1. **Handles Complex Formulas**: MCMC works well even for highly nonlinear or complex formulas where analytical error propagation is difficult.
2. **Correlated Errors**: MCMC can naturally handle correlated errors in the input variables by sampling from the joint distribution.
3. **Non-Gaussian Distributions**: MCMC can handle non-Gaussian uncertainties in the input variables.
4. **Flexibility**: You can incorporate prior knowledge or constraints into the MCMC model.

---

### **Example**
Suppose your formula is:
\[
y = x_1 \cdot x_2 + x_3^2
\]
and the input variables \( x_1, x_2, x_3 \) have Gaussian uncertainties:
\[
x_1 \sim \mathcal{N}(\mu_1, \sigma_1), \quad
x_2 \sim \mathcal{N}(\mu_2, \sigma_2), \quad
x_3 \sim \mathcal{N}(\mu_3, \sigma_3)
\]

1. Define the likelihood for each input variable.
2. Use MCMC to sample from the joint distribution of \( x_1, x_2, x_3 \).
3. For each sample, compute \( y = x_1 \cdot x_2 + x_3^2 \).
4. Plot a histogram of the \( y \) values to visualize the error distribution.

---

### **Tools for MCMC**
- **Python Libraries**:
  - `emcee`: A popular MCMC library for Python.
  - `PyMC3` or `PyMC4`: Probabilistic programming libraries with built-in MCMC samplers.
  - `Stan`: A probabilistic programming language with Python interfaces (`pystan`, `cmdstanpy`).
- **R Libraries**:
  - `rstan`: Interface to Stan for R.
  - `MCMCpack`: A general-purpose MCMC library for R.

---

### **When to Use MCMC**
- Use MCMC when:
  - The formula is complex or nonlinear.
  - The input uncertainties are correlated or non-Gaussian.
  - You want a full posterior distribution of the output error (not just mean and standard deviation).

If your problem is simple (e.g., linear formula with Gaussian uncertainties), analytical error propagation might be sufficient. However, for more complex cases, MCMC is a robust and flexible tool.

Certainly! Below is an example of how you can use **Turing.jl**, a probabilistic programming library in Julia, to perform MCMC sampling and calculate the error histogram for a formula. In this example, we'll use the formula:

\[
y = x_1 \cdot x_2 + x_3^2
\]

where \( x_1, x_2, x_3 \) are input variables with Gaussian uncertainties.

---

### **Example Code in Julia (Turing.jl)**

```julia
using Turing
using StatsPlots
using Random

# Define the formula
function formula(x1, x2, x3)
    return x1 * x2 + x3^2
end

# Define the probabilistic model
@model function error_propagation_model(μ1, μ2, μ3, σ1, σ2, σ3)
    # Priors for the input variables (Gaussian distributions)
    x1 ~ Normal(μ1, σ1)
    x2 ~ Normal(μ2, σ2)
    x3 ~ Normal(μ3, σ3)
    
    # Compute the output
    y = formula(x1, x2, x3)
    
    # Return the output (optional: you can also return the inputs if needed)
    return y
end

# Parameters for the input distributions
μ1 = 10.0  # Mean of x1
μ2 = 5.0   # Mean of x2
μ3 = 2.0   # Mean of x3
σ1 = 0.5   # Standard deviation of x1
σ2 = 0.3   # Standard deviation of x2
σ3 = 0.2   # Standard deviation of x3

# Instantiate the model
model = error_propagation_model(μ1, μ2, μ3, σ1, σ2, σ3)

# Run MCMC sampling
chain = sample(model, NUTS(), 10_000)  # Use NUTS sampler with 10,000 samples

# Extract the output values (y) from the chain
y_samples = chain[:y].data

# Plot the histogram of the output error
histogram(y_samples, bins=30, label="y", xlabel="Output Value (y)", ylabel="Frequency", title="Error Histogram")

# Print summary statistics
println("Mean of y: ", mean(y_samples))
println("Standard deviation of y: ", std(y_samples))
```

---

### **Explanation of the Code**

1. **Formula Definition**:
   - The `formula` function computes the output \( y = x_1 \cdot x_2 + x_3^2 \).

2. **Probabilistic Model**:
   - The `@model` macro defines a probabilistic model where:
     - \( x_1, x_2, x_3 \) are sampled from Gaussian distributions with means \( \mu_1, \mu_2, \mu_3 \) and standard deviations \( \sigma_1, \sigma_2, \sigma_3 \).
     - The output \( y \) is computed using the formula.

3. **MCMC Sampling**:
   - The `sample` function runs the MCMC sampler (NUTS in this case) to generate samples from the posterior distribution of \( y \).

4. **Error Histogram**:
   - The `histogram` function plots the distribution of the output values \( y \).

5. **Summary Statistics**:
   - The mean and standard deviation of \( y \) are computed and printed.

---

### **Output**
- The code will generate a histogram of the output values \( y \), showing the distribution of the error.
- It will also print the mean and standard deviation of \( y \).

---

### **Dependencies**
Make sure you have the required Julia packages installed:
```julia
using Pkg
Pkg.add("Turing")
Pkg.add("StatsPlots")
```

---

This code provides a flexible framework for propagating uncertainties through a formula using MCMC. You can adapt it to more complex formulas or different input distributions as needed.

