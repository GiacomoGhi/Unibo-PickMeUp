// Global variables passed from the view
let googleClientId = "";
let isAuthenticated = false;

// Helper per gestire la risposta del controller
function handleControllerResult(result) {
  // ControllerResult ritorna: { isSuccess, data, errorMessage }
  if (!result.isSuccess && result.errorMessage) {
    utilities.alertError(result.errorMessage, 3000);
  }
  return result;
}

// Google Sign-In callback
async function handleGoogleCallback(response) {
  if (!response.credential) return;

  // Get the URL from the data attribute
  const googleLoginUrl =
    document.getElementById("googleLoginBtn")?.dataset.url ||
    document.getElementById("googleSignupBtn")?.dataset.url ||
    "/Auth/GoogleLogin";

  try {
    const result = await utilities.postJsonT(googleLoginUrl, {
      credential: response.credential,
    });

    handleControllerResult(result);

    if (result.isSuccess && result.data?.redirectUrl) {
      window.location.href = result.data.redirectUrl;
    }
  } catch (error) {
    console.error("Google login error:", error);
    utilities.alertError("Errore di connessione. Riprova.", 3000);
  }
}

// Login form handler
document
  .getElementById("loginForm")
  ?.addEventListener("submit", async function (e) {
    e.preventDefault();
    const formData = new FormData(this);
    const data = Object.fromEntries(formData.entries());
    const url = this.dataset.url || "/Auth/Login";

    try {
      const result = await utilities.postJsonT(url, data);

      handleControllerResult(result);

      if (result.isSuccess && result.data?.redirectUrl) {
        window.location.href = result.data.redirectUrl;
      }
    } catch (error) {
      console.error("Login error:", error);
      utilities.alertError("Errore di connessione. Riprova.", 3000);
    }
  });

// Signup form handler
document
  .getElementById("signupForm")
  ?.addEventListener("submit", async function (e) {
    e.preventDefault();
    const formData = new FormData(this);
    const data = Object.fromEntries(formData.entries());
    const url = this.dataset.url || "/Auth/SignUp";

    // Validate password confirmation
    if (data.Password !== data.ConfirmPassword) {
      utilities.alertError("Le password non corrispondono", 3000);
      return;
    }

    try {
      const result = await utilities.postJsonT(url, data);

      handleControllerResult(result);

      if (result.isSuccess) {
        this.reset();
        bootstrap.Modal.getInstance(
          document.getElementById("signupModal")
        )?.hide();
      }
    } catch (error) {
      console.error("Signup error:", error);
      utilities.alertError("Errore di connessione. Riprova.", 3000);
    }
  });

// Resend confirmation form handler
document
  .getElementById("resendForm")
  ?.addEventListener("submit", async function (e) {
    e.preventDefault();
    const formData = new FormData(this);
    const data = Object.fromEntries(formData.entries());
    const url = this.dataset.url || "/Auth/ResendConfirmation";

    try {
      const result = await utilities.postJsonT(url, data);

      handleControllerResult(result);

      if (result.isSuccess) {
        this.reset();
        bootstrap.Modal.getInstance(
          document.getElementById("resendModal")
        )?.hide();
      }
    } catch (error) {
      console.error("Resend error:", error);
      utilities.alertError("Errore di connessione. Riprova.", 3000);
    }
  });

// Reset password form handler
document
  .getElementById("resetForm")
  ?.addEventListener("submit", async function (e) {
    e.preventDefault();
    const formData = new FormData(this);
    const data = Object.fromEntries(formData.entries());
    const url = this.dataset.url || "/Auth/RequestPasswordReset";

    try {
      const result = await utilities.postJsonT(url, data);

      handleControllerResult(result);

      if (result.isSuccess) {
        this.reset();
        bootstrap.Modal.getInstance(
          document.getElementById("resetPasswordModal")
        )?.hide();
      }
    } catch (error) {
      console.error("Reset password error:", error);
      utilities.alertError("Errore di connessione. Riprova.", 3000);
    }
  });

// Handle "Trova Passaggio" button
function handleFindRide() {
  if (isAuthenticated) {
    window.location.href = "/Search/FindRide";
  } else {
    utilities.alertError(
      "Devi effettuare l'accesso per cercare un passaggio",
      3000
    );
    new bootstrap.Modal(document.getElementById("loginModal")).show();
  }
}

// Handle "Aggiungi Viaggio" button
function handleAddTrip() {
  if (isAuthenticated) {
    window.location.href = "/Trip/Add";
  } else {
    utilities.alertError(
      "Devi effettuare l'accesso per aggiungere un viaggio",
      3000
    );
    new bootstrap.Modal(document.getElementById("loginModal")).show();
  }
}

// Initialize everything on window load
window.addEventListener("load", function () {
  const bodyData = document.body.dataset;
  isAuthenticated = bodyData.isAuthenticated === "true";
  googleClientId = bodyData.googleClientId || "";

  if (typeof google !== "undefined" && googleClientId) {
    google.accounts.id.initialize({
      client_id: googleClientId,
      callback: handleGoogleCallback,
    });

    const loginBtn = document.getElementById("googleLoginBtn");
    if (loginBtn) {
      google.accounts.id.renderButton(loginBtn, {
        theme: "outline",
        size: "large",
        width: loginBtn.offsetWidth,
      });
    }

    const signupBtn = document.getElementById("googleSignupBtn");
    if (signupBtn) {
      google.accounts.id.renderButton(signupBtn, {
        theme: "outline",
        size: "large",
        width: signupBtn.offsetWidth,
      });
    }
  }
});
