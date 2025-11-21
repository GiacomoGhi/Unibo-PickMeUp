// ============================================================================
// LANDING PAGE - VUE.JS IMPLEMENTATION
// ============================================================================

// La funzione viene chiamata dal callback dell'API di Google Maps
function initVueApp() {
  const { createApp, ref, reactive, computed, onMounted, watch } = Vue;

  createApp({
    setup() {
      // -- STATE --
      const state = reactive({
        departure: null,
        destination: null,
        departureDate: null,
        departureTime: null,
        submitted: false,
      });

      let departureAutocomplete;
      let destinationAutocomplete;

      // -- COMPUTED PROPERTIES --
      const isDepartureValid = computed(() => !!state.departure);
      const isDestinationValid = computed(() => !!state.destination);
      const arePlacesValid = computed(
        () => isDepartureValid.value && isDestinationValid.value
      );

      // -- METHODS --
      const initGooglePlaces = async () => {
        const { PlaceAutocompleteElement } = await google.maps.importLibrary(
          "places"
        );

        // Inizializza autocomplete per la partenza
        departureAutocomplete = new PlaceAutocompleteElement({
          componentRestrictions: { country: "it" },
        });
        departureAutocomplete.id = "departure-autocomplete";
        departureAutocomplete.placeholder = "Inserisci indirizzo di partenza";
        document
          .getElementById("departure-container")
          .appendChild(departureAutocomplete);
        departureAutocomplete.addEventListener(
          "gmp-select",
          async ({ placePrediction }) =>
            await onPlaceChangedAsync("departure", placePrediction)
        );

        // Inizializza autocomplete per la destinazione
        destinationAutocomplete = new PlaceAutocompleteElement({
          componentRestrictions: { country: "it" },
        });
        destinationAutocomplete.id = "destination-autocomplete";
        destinationAutocomplete.placeholder =
          "Inserisci indirizzo di destinazione";
        document
          .getElementById("destination-container")
          .appendChild(destinationAutocomplete);
        destinationAutocomplete.addEventListener(
          "gmp-select",
          async ({ placePrediction }) =>
            await onPlaceChangedAsync("destination", placePrediction)
        );
      };

      const onPlaceChangedAsync = async (type, placePrediction) => {
        const place = placePrediction.toPlace();
        await place.fetchFields({
          fields: [
            "formattedAddress",
            "location",
            "displayName",
            "id",
            "addressComponents",
          ],
        });

        if (!place.location) {
          console.warn(`No location available for: '${place.displayName}'`);
          state[type] = null;
          return;
        }

        state[type] = {
          placeId: place.id,
          formattedAddress: place.formattedAddress,
          name: place.displayName,
          coordinates: {
            lat: place.location.lat(),
            lng: place.location.lng(),
          },
          addressComponents: extractAddressComponents(place.addressComponents),
        };
      };

      const extractAddressComponents = (components) => {
        const result = {
          street: "",
          city: "",
          province: "",
          region: "",
          country: "",
          postalCode: "",
        };
        if (!components) return result;
        components.forEach((c) => {
          if (c.types.includes("route")) result.street = c.long_name;
          if (c.types.includes("locality")) result.city = c.long_name;
          if (c.types.includes("administrative_area_level_2"))
            result.province = c.short_name;
          if (c.types.includes("administrative_area_level_1"))
            result.region = c.long_name;
          if (c.types.includes("country")) result.country = c.long_name;
          if (c.types.includes("postal_code")) result.postalCode = c.long_name;
        });
        return result;
      };

      const validateAndGetData = () => {
        state.submitted = true;

        if (!arePlacesValid.value) {
          return null;
        }

        return {
          departure: state.departure,
          destination: state.destination,
          departureDate: state.departureDate,
          departureTime: state.departureTime,
          timestamp: new Date().toISOString(),
        };
      };

      const handleFindRide = () => {
        if (typeof isAuthenticated !== "undefined" && !isAuthenticated) {
          utilities.alertError(
            "Devi effettuare l'accesso per cercare un passaggio",
            3000
          );
          new bootstrap.Modal(document.getElementById("loginModal")).show();
          return;
        }

        const searchData = validateAndGetData();
        if (!searchData) {
          utilities.alertError(
            "Seleziona sia la partenza che la destinazione dai suggerimenti",
            3000
          );
          return;
        }

        const params = new URLSearchParams({
          DeparturePlaceId: searchData.departure.placeId,
          DepartureAddress: searchData.departure.formattedAddress,
          DepartureLat: searchData.departure.coordinates.lat,
          DepartureLng: searchData.departure.coordinates.lng,
          DestinationPlaceId: searchData.destination.placeId,
          DestinationAddress: searchData.destination.formattedAddress,
          DestinationLat: searchData.destination.coordinates.lat,
          DestinationLng: searchData.destination.coordinates.lng,
        });

        window.location.href = `/Travel/Search?${params.toString()}`;
      };

      const handleAddTrip = () => {
        if (typeof isAuthenticated !== "undefined" && !isAuthenticated) {
          utilities.alertError(
            "Devi effettuare l'accesso per aggiungere un viaggio",
            3000
          );
          new bootstrap.Modal(document.getElementById("loginModal")).show();
          return;
        }

        const searchData = validateAndGetData();
        if (!searchData) {
          utilities.alertError(
            "Seleziona sia la partenza che la destinazione dai suggerimenti",
            3000
          );
          return;
        }

        sessionStorage.setItem("newTripData", JSON.stringify(searchData));
        window.location.href = "/Trip/Create"; // Reindirizzamento diretto
      };

      // -- WATCHERS --
      // Osserva i cambiamenti per applicare le classi di validazione in modo reattivo
      watch(
        () => state.departure,
        (newValue) => {
          if (departureAutocomplete) {
            departureAutocomplete.classList.toggle("is-valid", !!newValue);
            departureAutocomplete.classList.remove("is-invalid");
          }
        }
      );

      watch(
        () => state.destination,
        (newValue) => {
          if (destinationAutocomplete) {
            destinationAutocomplete.classList.toggle("is-valid", !!newValue);
            destinationAutocomplete.classList.remove("is-invalid");
          }
        }
      );

      // Osserva lo stato di 'submitted' per mostrare gli errori
      watch(
        () => state.submitted,
        (isSubmitted) => {
          if (isSubmitted) {
            if (!isDepartureValid.value && departureAutocomplete) {
              departureAutocomplete.classList.add("is-invalid");
            }
            if (!isDestinationValid.value && destinationAutocomplete) {
              destinationAutocomplete.classList.add("is-invalid");
            }
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
        isDepartureValid,
        isDestinationValid,
        handleFindRide,
        handleAddTrip,
      };
    },
  }).mount("#searchApp");
}

// Gestisce il caso in cui l'utente effettua il login
window.addEventListener("userLoggedIn", () => {
  console.log("User logged in successfully, form data preserved by Vue state.");
});
