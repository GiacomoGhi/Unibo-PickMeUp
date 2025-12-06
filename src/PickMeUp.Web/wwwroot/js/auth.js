// Google Sign-In callback
async function handleGoogleCallback(response) {
  if (!response.credential) return;

  // Get the URL from the data attribute
  const googleLoginUrl =
    document.getElementById("googleLoginBtn")?.dataset.url ||
    "/Auth/GoogleLogin";

  const returnUrl =
    document.getElementById("googleLoginBtn")?.dataset.returnUrl || "/";

  try {
    const result = await utilities.postJsonT(googleLoginUrl, {
      credential: response.credential,
    });

    // ControllerResult ritorna: { isSuccess, data, errorMessage }
    if (!result.isSuccess) {
      utilities.alertError(result.errorMessage, 3000);
    }

    if (result.isSuccess) {
      // Show success message
      utilities.alertSuccess("Login effettuato con successo!", 3000);

      // Redirect to return URL
      window.location.href = returnUrl;
    }
  } catch (error) {
    console.error("Google login error:", error);
    utilities.alertError("Errore di connessione. Riprova.", 3000);
  }
}

// Initialize everything on window load
window.addEventListener("load", function () {
  const loginBtn = document.getElementById("googleLoginBtn");

  if (!loginBtn) {
    console.warn("Google login button not found.");
    return;
  }
  googleClientId = loginBtn.dataset.googleClientId || "";

  if (typeof google == "undefined" || !googleClientId) {
    console.warn("Google Identity Services not loaded or client ID missing.");
    return;
  }

  google.accounts.id.initialize({
    client_id: googleClientId,
    callback: handleGoogleCallback,
  });

  if (loginBtn) {
    google.accounts.id.renderButton(loginBtn, {
      theme: "outline",
      size: "large",
      width: loginBtn.offsetWidth,
    });
  }
});
