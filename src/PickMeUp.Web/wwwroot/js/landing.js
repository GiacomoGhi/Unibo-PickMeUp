let departureAutocomplete;
let destinationAutocomplete;

function initAutocomplete() {
  const departureInput = document.getElementById("departure");
  const destinationInput = document.getElementById("destination");

  const options = {
    // Puoi limitare i risultati a una nazione specifica, es. Italia
    componentRestrictions: { country: "it" },
    // Specifica quali dati richiedere per non pagare per dati non necessari
    fields: ["address_components", "geometry", "icon", "name", "place_id"],
  };

  departureAutocomplete = new google.maps.places.Autocomplete(
    departureInput,
    options
  );
  destinationAutocomplete = new google.maps.places.Autocomplete(
    destinationInput,
    options
  );

  // Puoi aggiungere listener per quando un utente seleziona un luogo, se necessario
  // departureAutocomplete.addListener('place_changed', onPlaceChanged);
  // destinationAutocomplete.addListener('place_changed', onPlaceChanged);
}

function handleFindRide() {
  // Check if user is authenticated (this function is in auth.js)
  if (typeof isAuthenticated !== "undefined" && !isAuthenticated) {
    utilities.alertError(
      "Devi effettuare l'accesso per cercare un passaggio",
      3000
    );
    new bootstrap.Modal(document.getElementById("loginModal")).show();
    return;
  }

  const destination = document.getElementById("destination").value;
  const departure = document.getElementById("departure").value;

  if (!destination || !departure) {
    alert("Per favore, inserisci sia la partenza che la destinazione.");
    return;
  }

  // Proceed with search
  // window.location.href = `/Search/Results?departure=${encodeURIComponent(departure)}&destination=${encodeURIComponent(destination)}`;
  console.log("Finding ride from", departure, "to", destination);
}

function handleAddTrip() {
  // Check if user is authenticated
  if (typeof isAuthenticated !== "undefined" && !isAuthenticated) {
    utilities.alertError(
      "Devi effettuare l'accesso per aggiungere un viaggio",
      3000
    );
    new bootstrap.Modal(document.getElementById("loginModal")).show();
    return;
  }

  const destination = document.getElementById("destination").value;
  const departure = document.getElementById("departure").value;

  if (!destination || !departure) {
    alert("Per favore, inserisci sia la partenza che la destinazione.");
    return;
  }

  // Reindirizza alla pagina di creazione viaggio, passando i valori
  window.location.href = `/Trip/Create?departure=${encodeURIComponent(
    departure
  )}&destination=${encodeURIComponent(destination)}`;
  console.log("Adding trip from", departure, "to", destination);
}

// Listen for login success event
window.addEventListener("userLoggedIn", function () {
  console.log("User logged in successfully, form data preserved");
});
