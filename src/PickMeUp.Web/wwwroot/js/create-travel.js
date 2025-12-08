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
        seatsCount: 1,
        departureDate: null,
        departureTime: null,
        submitted: false,
      });

      let departureAutocomplete;
      let destinationAutocomplete;

      // -- COMPUTED PROPERTIES --
      const isDepartureValid = computed(() => !!state.departure);
      const isDestinationValid = computed(() => !!state.destination);
      const isSeatsValid = computed(() => {
        const n = Number(state.seatsCount);
        return Number.isInteger(n) && n >= 1;
      });
      const arePlacesValid = computed(
        () =>
          isDepartureValid.value &&
          isDestinationValid.value &&
          isSeatsValid.value
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
        // Ensure autocomplete element gets Bootstrap form styling
        departureAutocomplete.className = "form-control bg-pmu";
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
        // Ensure autocomplete element gets Bootstrap form styling
        destinationAutocomplete.className = "form-control bg-pmu";
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
          street_number: "",
          route: "",
          city: "",
          postalCode: "",
          province: "",
          region: "",
          country: "",
          continent: "",
        };

        if (!components) return result;

        components.forEach((c) => {
          if (c.types.includes("street_number")) {
            result.street_number = c.longText;
          }
          if (c.types.includes("route")) {
            result.route = c.longText;
          }
          if (c.types.includes("locality")) {
            result.city = c.longText;
          }
          if (c.types.includes("postal_code")) {
            result.postalCode = c.longText;
          }
          if (c.types.includes("administrative_area_level_2")) {
            result.province = c.shortText;
          }
          if (c.types.includes("administrative_area_level_1")) {
            result.region = c.longText;
          }
          if (c.types.includes("country")) {
            result.country = c.longText;
          }
          if (c.types.includes("continent")) {
            result.continent = c.longText;
          }
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
          seatsCount: Number(state.seatsCount),
        };
      };

      const handleAddTrip = async () => {
        const searchData = validateAndGetData();
        if (!searchData) {
          utilities.alertError(
            "Seleziona sia la partenza che la destinazione dai suggerimenti",
            3000
          );
          return;
        }

        const payload = {
          UserId: 0,
          UserTravelId: 0,
          DepartureDate: searchData.departureDate || null,
          DepartureTime: searchData.departureTime || null,
          DepartureLocation: {
            LocationId: 0,
            ReadableAddress: searchData.departure.formattedAddress,
            Coordinates: {
              Latitude: searchData.departure.coordinates.lat,
              Longitude: searchData.departure.coordinates.lng,
            },
            Street: searchData.departure.addressComponents.route,
            Number: searchData.departure.addressComponents.street_number,
            City: searchData.departure.addressComponents.city,
            PostalCode: searchData.departure.addressComponents.postalCode,
            Province: searchData.departure.addressComponents.province,
            Region: searchData.departure.addressComponents.region,
            Country: searchData.departure.addressComponents.country,
            Continent: searchData.departure.addressComponents.continent,
          },
          DestinationLocation: {
            LocationId: 0,
            ReadableAddress: searchData.destination.formattedAddress,
            Coordinates: {
              Latitude: searchData.destination.coordinates.lat,
              Longitude: searchData.destination.coordinates.lng,
            },
            Street: searchData.destination.addressComponents.route,
            Number: searchData.destination.addressComponents.street_number,
            City: searchData.destination.addressComponents.city,
            PostalCode: searchData.destination.addressComponents.postalCode,
            Province: searchData.destination.addressComponents.province,
            Region: searchData.destination.addressComponents.region,
            Country: searchData.destination.addressComponents.country,
            Continent: searchData.destination.addressComponents.continent,
          },
          TotalPassengersSeatsCount: searchData.seatsCount,
          OccupiedPassengerSeatsCount: 0,
          PickUpRequests: [],
        };

        try {
          const result = await utilities.postJsonT("/Travel/Edit", payload);
          // If controller returns a standard ControllerResult
          if (result && result.isSuccess) {
            utilities.alertSuccess("Viaggio creato (richiesta inviata)", 2500);
            // Redirect to the newly created travel page
            window.location.href = `/Travel/Travel?travelId=${result.data.travelId}`;
          } else {
            // Show server-provided error if present
            const msg =
              (result && result.errorMessage) ||
              "Errore durante la creazione del viaggio";
            utilities.alertError(msg, 3000);
          }
        } catch (err) {
          console.error("Add trip error:", err);
          utilities.alertError("Errore di connessione. Riprova.", 3000);
        }
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
        isSeatsValid,
        handleAddTrip,
      };
    },
  }).mount("#createApp");
}
