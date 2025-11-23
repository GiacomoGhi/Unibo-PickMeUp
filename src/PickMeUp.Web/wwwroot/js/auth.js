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

    if (result.isSuccess) {
      // Update authentication state
      isAuthenticated = true;

      // Close any open modal
      const loginModal = bootstrap.Modal.getInstance(
        document.getElementById("loginModal")
      );
      const signupModal = bootstrap.Modal.getInstance(
        document.getElementById("signupModal")
      );
      if (loginModal) loginModal.hide();
      if (signupModal) signupModal.hide();

      // Show success message
      utilities.alertSuccess("Login effettuato con successo!", 3000);

      // Update UI without page reload
      updateAuthUI();
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

      if (result.isSuccess) {
        // Update authentication state
        isAuthenticated = true;

        // Close the modal
        const loginModal = bootstrap.Modal.getInstance(
          document.getElementById("loginModal")
        );
        if (loginModal) {
          loginModal.hide();
        }

        // Show success message
        utilities.alertSuccess("Login effettuato con successo!", 3000);

        // Update UI without page reload
        updateAuthUI();
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
        const signupModal = bootstrap.Modal.getInstance(
          document.getElementById("signupModal")
        );

        if (signupModal) {
          signupModal.hide();

          // Wait for modal to close, then show info modal
          document.getElementById("signupModal").addEventListener(
            "hidden.bs.modal",
            function () {
              new bootstrap.Modal(
                document.getElementById("accountActivationModal")
              ).show();
            },
            { once: true }
          );
        }
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
        const resendModal = bootstrap.Modal.getInstance(
          document.getElementById("resendModal")
        );

        if (resendModal) {
          resendModal.hide();

          // Wait for modal to close, then show info modal
          document.getElementById("resendModal").addEventListener(
            "hidden.bs.modal",
            function () {
              new bootstrap.Modal(
                document.getElementById("accountActivationModal")
              ).show();
            },
            { once: true }
          );
        }
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
        const resetModal = bootstrap.Modal.getInstance(
          document.getElementById("resetPasswordModal")
        );

        if (resetModal) {
          resetModal.hide();

          // Wait for modal to close, then show info modal
          document.getElementById("resetPasswordModal").addEventListener(
            "hidden.bs.modal",
            function () {
              new bootstrap.Modal(
                document.getElementById("passwordResetSentModal")
              ).show();
            },
            { once: true }
          );
        }
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

// Update UI elements after login without page reload
function updateAuthUI() {
  // Replace the auth area by asking the server who is the current user.
  // This ensures the server-side cookie is used and we get the correct name.
  fetch("/Auth/CurrentUser", { credentials: "same-origin" })
    .then((r) => r.json())
    .then((data) => {
      const authSection = document.getElementById("authSection");
      if (!authSection) return;

      if (data && data.isAuthenticated) {
        // Render a simple dropdown matching the server-side layout
        authSection.innerHTML = `
          <div class="dropdown">
            <button class="btn p-0 dropdown-toggle d-flex flex-column align-items-center bg-transparent" type="button" id="dropdownUserActions" data-bs-toggle="dropdown" aria-expanded="false">
              <img src="/images/avatar.png" alt="Avatar" style="height: 80px;" />
              <p class="fs-3 fw-bold m-0">${escapeHtml(
                data.firstName || ""
              )}</p>
            </button>
            <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="dropdownUserActions">
              <li><a class="dropdown-item" href="#" data-bs-toggle="modal" data-bs-target="#logoutModal">Log out</a></li>
            </ul>
          </div>
        `;
      } else {
        authSection.innerHTML = `
          <button id="loginTriggerBtn" class="btn p-0 d-flex flex-column align-items-center bg-transparent" data-bs-toggle="modal" data-bs-target="#loginModal">
            <img src="/images/avatar.png" alt="Login" style="height: 80px;" />
            <p class="fs-3 fw-bold m-0">Accedi</p>
          </button>
        `;
      }

      // Mark local state and notify listeners
      isAuthenticated = !!(data && data.isAuthenticated);
      window.dispatchEvent(new CustomEvent("userLoggedIn"));
    })
    .catch((err) => {
      console.error("Failed to refresh auth UI:", err);
    });
}

// Minimal HTML-escape helper for insertion into template strings
function escapeHtml(unsafe) {
  if (!unsafe) return "";
  return String(unsafe)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/\"/g, "&quot;")
    .replace(/'/g, "&#039;");
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
