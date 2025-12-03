// ============================================================================
// SEARCH TRAVEL - VUE.JS IMPLEMENTATION
// ============================================================================

function initSearchApp() {
  const { createApp, reactive, onMounted, watch, computed } = Vue;

  createApp({
    setup() {
      // -- STATE --
      // Initialize state from server-side data if available
      const initialFilters = window.initialFilters || {};
      const state = reactive({
        departure: initialFilters.departure || null,
        destination: initialFilters.destination || null,
        departureDate: initialFilters.departureDate || "",
      });

      let departureAutocomplete;
      let destinationAutocomplete;
      const searchForm = Vue.ref(null);

      // -- COMPUTED --
      // Allow search if at least one field is filled, or whatever logic you prefer
      const isSearchValid = computed(
        () => !!state.departure || !!state.destination
      );

      // -- METHODS --
      const initGooglePlaces = async () => {
        const { PlaceAutocompleteElement } = await google.maps.importLibrary(
          "places"
        );

        // 1. Departure Autocomplete
        departureAutocomplete = new PlaceAutocompleteElement({
          componentRestrictions: { country: "it" },
        });
        departureAutocomplete.id = "departure-autocomplete";
        departureAutocomplete.placeholder = "Filtra per partenza...";
        departureAutocomplete.className = "form-control";

        const depContainer = document.getElementById("departure-container");
        if (depContainer) depContainer.appendChild(departureAutocomplete);

        departureAutocomplete.addEventListener(
          "gmp-select",
          async ({ placePrediction }) => {
            await onPlaceChangedAsync("departure", placePrediction);
          }
        );

        // Set initial value if available
        if (state.departure) {
          departureAutocomplete.value = state.departure.formattedAddress;
        }

        // 2. Destination Autocomplete
        destinationAutocomplete = new PlaceAutocompleteElement({
          componentRestrictions: { country: "it" },
        });
        destinationAutocomplete.id = "destination-autocomplete";
        destinationAutocomplete.placeholder = "Filtra per destinazione...";
        destinationAutocomplete.className = "form-control";

        const destContainer = document.getElementById("destination-container");
        if (destContainer) destContainer.appendChild(destinationAutocomplete);

        destinationAutocomplete.addEventListener(
          "gmp-select",
          async ({ placePrediction }) => {
            await onPlaceChangedAsync("destination", placePrediction);
          }
        );

        // Set initial value if available
        if (state.destination) {
          destinationAutocomplete.value = state.destination.formattedAddress;
        }
      };

      const onPlaceChangedAsync = async (type, placePrediction) => {
        // Handle "Clear" (x button)
        if (!placePrediction) {
          state[type] = null;
          return;
        }

        const place = placePrediction.toPlace();
        await place.fetchFields({
          fields: [
            "formattedAddress",
            "location",
            "displayName",
            "addressComponents",
          ],
        });

        if (!place.location) {
          state[type] = null;
          return;
        }

        state[type] = {
          formattedAddress: place.formattedAddress,
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
          if (c.types.includes("street_number"))
            result.street_number = c.longText;
          if (c.types.includes("route")) result.route = c.longText;
          if (c.types.includes("locality")) result.city = c.longText;
          if (c.types.includes("postal_code")) result.postalCode = c.longText;
          if (c.types.includes("administrative_area_level_2"))
            result.province = c.shortText;
          if (c.types.includes("administrative_area_level_1"))
            result.region = c.longText;
          if (c.types.includes("country")) result.country = c.longText;
          if (c.types.includes("continent")) result.continent = c.longText;
        });

        return result;
      };

      const handleSearch = () => {
        // Because we are using hidden inputs bound to Vue state,
        // submitting the form natively will send all the data to the controller.
        if (searchForm.value) {
          searchForm.value.submit();
        }
      };

      // -- LIFECYCLE --
      onMounted(() => {
        initGooglePlaces();
      });

      return {
        state,
        searchForm,
        isSearchValid,
        handleSearch,
      };
    },
  }).mount("#searchApp");
}
