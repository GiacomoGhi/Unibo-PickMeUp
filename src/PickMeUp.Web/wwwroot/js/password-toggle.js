// Password toggle visibility
(function () {
  // Initialize password toggles
  function initPasswordToggles() {
    const toggleButtons = document.querySelectorAll(".pmu-password-toggle");

    toggleButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const targetId = this.dataset.target;
        const input = document.getElementById(targetId);
        const eyeOpen = this.querySelector(".eye-open");
        const eyeClosed = this.querySelector(".eye-closed");

        if (input && eyeOpen && eyeClosed) {
          if (input.type === "password") {
            input.type = "text";
            eyeOpen.style.display = "none";
            eyeClosed.style.display = "block";
          } else {
            input.type = "password";
            eyeOpen.style.display = "block";
            eyeClosed.style.display = "none";
          }
        }
      });
    });
  }

  // Initialize on page load
  window.addEventListener("load", initPasswordToggles);

  // Re-initialize when modals are shown (for dynamically loaded content)
  document.addEventListener("shown.bs.modal", initPasswordToggles);
})();
