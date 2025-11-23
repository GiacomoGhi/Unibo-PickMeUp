// ============================================================================
// TRAVEL PAGE - GOOGLE MAPS ROUTE DISPLAY
// ============================================================================
// Mostra il percorso tra partenza e destinazione su Google Maps
// ============================================================================

let map;
let directionsService;
let directionsRenderer;

async function initMap() {
  const { Map } = await google.maps.importLibrary("maps");

  const routeData = window.routeData;

  if (!routeData) {
    console.error("Nessun dato di percorso disponibile");
    return;
  }

  const centerLat = (routeData.origin.lat + routeData.destination.lat) / 2;
  const centerLng = (routeData.origin.lng + routeData.destination.lng) / 2;

  map = new Map(document.getElementById("map"), {
    center: { lat: centerLat, lng: centerLng },
    zoom: 8,
    mapTypeControl: false,
    fullscreenControl: true,
    streetViewControl: false,
  });

  // Marker partenza/destinazione
  new google.maps.Marker({
    position: { lat: routeData.origin.lat, lng: routeData.origin.lng },
    map,
    label: "A",
  });

  new google.maps.Marker({
    position: {
      lat: routeData.destination.lat,
      lng: routeData.destination.lng,
    },
    map,
    label: "B",
  });

  // Aggiungi marker per ogni pick-up point
  if (routeData.pickUpRequests && Array.isArray(routeData.pickUpRequests)) {
    routeData.pickUpRequests.forEach((request, index) => {
      if (
        request.location &&
        Number.isFinite(request.location.lat) &&
        Number.isFinite(request.location.lng)
      ) {
        new google.maps.Marker({
          position: { lat: request.location.lat, lng: request.location.lng },
          map,
          label: `${index + 1}`,
        });
      }
    });
  }

  // Polyline percorso
  if (!routeData.encodedPolyline) {
    console.error("Nessun polilinea codificato disponibile per il percorso");
    return;
  }

  const { encoding } = await google.maps.importLibrary("geometry");
  const path = encoding.decodePath(routeData.encodedPolyline);

  const polyline = new google.maps.Polyline({
    path,
    map,
    strokeColor: "#3b82f6",
    strokeWeight: 5,
    strokeOpacity: 0.8,
  });

  // Fit ai bounds del percorso
  const bounds = new google.maps.LatLngBounds();
  path.forEach((p) => bounds.extend(p));
  map.fitBounds(bounds);

  // Info distanza/durata se presenti
  if (routeData.distanceMeters && routeData.durationSeconds) {
    displayRouteInfoFromMetrics(
      routeData.distanceMeters,
      routeData.durationSeconds
    );
  }
}

// TODO delete
function displayRouteInfoFromMetrics(distanceMeters, durationSeconds) {
  if (!Number.isFinite(distanceMeters) || !Number.isFinite(durationSeconds)) {
    return;
  }

  const km = (distanceMeters / 1000).toFixed(1);

  const minutesTotal = Math.round(durationSeconds / 60);
  const hours = Math.floor(minutesTotal / 60);
  const minutes = minutesTotal % 60;

  const durationText = hours > 0 ? `${hours}h ${minutes}m` : `${minutes}m`;

  const infoDiv = document.createElement("div");
  infoDiv.className = "pmu-route-info";
  infoDiv.innerHTML = `
    <div class="pmu-route-stats">
      <div class="pmu-stat">
        <i class="fa-solid fa-road"></i>
        <span>${km} km</span>
      </div>
      <div class="pmu-stat">
        <i class="fa-solid fa-clock"></i>
        <span>${durationText}</span>
      </div>
    </div>
  `;

  const mapElement = document.getElementById("map");
  mapElement.parentElement.insertBefore(infoDiv, mapElement);

  // -- STATE --
  const state = reactive({
    location: null,
    submitted: false,
  });

  const isLocationValid = computed(() => !!state.location);

  const initGooglePlaces = async () => {
    const { PlaceAutocompleteElement } = await google.maps.importLibrary(
      "places"
    );

    // Inizializza autocomplete per la location
    locationAutocomplete = new PlaceAutocompleteElement({
      componentRestrictions: { country: "it" },
    });
    locationAutocomplete.id = "location-autocomplete";
    locationAutocomplete.placeholder = "Inserisci indirizzo di pick-up";
    // Ensure autocomplete element gets Bootstrap form styling
    locationAutocomplete.className = "form-control";
    document
      .getElementById("location-container")
      .appendChild(locationAutocomplete);
    locationAutocomplete.addEventListener(
      "gmp-select",
      async ({ placePrediction }) =>
        await onPlaceChangedAsync("location", placePrediction)
    );
  };

  const validateAndGetData = () => {
    state.submitted = true;

    if (!isLocationValid.value) {
      return null;
    }

    return {
      location: state.location,
    };
  };

  const handleRequestPickUp = async (travelId) => {
    const searchData = validateAndGetData();
    if (!searchData) {
      utilities.alertError("Seleziona il pick-up point dai suggerimenti", 3000);
      return;
    }

    // Prepare payload matching query-string used in handleFindRide
    const payload = {
      TravelId: travelId,
      PickUpPointAddress: searchData.location.formattedAddress,
      Location: {
        Latitude: searchData.location.coordinates.lat,
        Longitude: searchData.location.coordinates.lng,
      },
    };

    try {
      await utilities.postJsonT("/Travel/EditPickUpRequest", payload);
    } catch (err) {
      console.error("Edit pick-up request error:", err);
      utilities.alertError("Errore di connessione. Riprova.", 3000);
    }
  };

  // -- WATCHERS --
  // Osserva i cambiamenti per applicare le classi di validazione in modo reattivo
  watch(
    () => state.location,
    (newValue) => {
      if (locationAutocomplete) {
        locationAutocomplete.classList.toggle("is-valid", !!newValue);
        locationAutocomplete.classList.remove("is-invalid");
      }
    }
  );

  // -- LIFECYCLE HOOK --
  onMounted(() => {
    initGooglePlaces();
  });

  // -- EXPOSE TO TEMPLATE --
  return {
    // Esponi l'intero oggetto di stato reattivo
    state,
    isLocationValid,
    handleRequestPickUp,
  };
}
