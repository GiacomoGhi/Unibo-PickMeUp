// ============================================================================
// TRAVEL PAGE - VUE.JS IMPLEMENTATION
// ============================================================================

// La funzione viene chiamata dal callback dell'API di Google Maps
function initTravelApp() {
  const { createApp, reactive, computed, onMounted, watch } = Vue;

  createApp({
    setup() {
      // -- STATE --
      const state = reactive({
        location: null, // Per la richiesta di pick-up
        submitted: false,
        routeStats: {
          distanceText: null,
          durationText: null,
          visible: false,
        },
      });

      let map = null;
      let locationAutocomplete = null;

      // -- COMPUTED PROPERTIES --
      const isLocationValid = computed(() => !!state.location);

      // -- METHODS: MAP --
      const initMap = async () => {
        const routeData = window.routeData;

        if (!routeData) {
          console.error("Nessun dato di percorso disponibile");
          return;
        }

        const { Map } = await google.maps.importLibrary("maps");
        const { encoding } = await google.maps.importLibrary("geometry");

        const centerLat =
          (routeData.origin.lat + routeData.destination.lat) / 2;
        const centerLng =
          (routeData.origin.lng + routeData.destination.lng) / 2;

        map = new Map(document.getElementById("map"), {
          center: { lat: centerLat, lng: centerLng },
          zoom: 8,
          mapTypeControl: false,
          fullscreenControl: true,
          streetViewControl: false,
        });

        // Marker Partenza (A)
        new google.maps.Marker({
          position: { lat: routeData.origin.lat, lng: routeData.origin.lng },
          map,
          label: "A",
        });

        // Marker Destinazione (B)
        new google.maps.Marker({
          position: {
            lat: routeData.destination.lat,
            lng: routeData.destination.lng,
          },
          map,
          label: "B",
        });

        // Marker Pick-up Requests esistenti
        if (
          routeData.pickUpRequests &&
          Array.isArray(routeData.pickUpRequests)
        ) {
          routeData.pickUpRequests.forEach((request, index) => {
            if (
              request.location &&
              Number.isFinite(request.location.lat) &&
              Number.isFinite(request.location.lng)
            ) {
              // Colora diversamente se accettato o pending (opzionale, qui semplice marker numerico)
              new google.maps.Marker({
                position: {
                  lat: request.location.lat,
                  lng: request.location.lng,
                },
                map,
                label: `${index + 1}`,
                title: request.UserNominative,
              });
            }
          });
        }

        // Polyline percorso
        if (routeData.encodedPolyline) {
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
        }

        // Imposta info distanza/durata nello stato reattivo
        if (routeData.distanceMeters && routeData.durationSeconds) {
          updateRouteStats(routeData.distanceMeters, routeData.durationSeconds);
        }
      };

      const updateRouteStats = (distanceMeters, durationSeconds) => {
        if (
          !Number.isFinite(distanceMeters) ||
          !Number.isFinite(durationSeconds)
        ) {
          return;
        }

        const km = (distanceMeters / 1000).toFixed(1);
        const minutesTotal = Math.round(durationSeconds / 60);
        const hours = Math.floor(minutesTotal / 60);
        const minutes = minutesTotal % 60;
        const durationText =
          hours > 0 ? `${hours}h ${minutes}m` : `${minutes}m`;

        state.routeStats.distanceText = `${km} km`;
        state.routeStats.durationText = durationText;
        state.routeStats.visible = true;
      };

      // -- METHODS: PLACES (Form) --
      const initGooglePlaces = async () => {
        // Verifica se esiste il container (potrebbe non esserci se sono l'owner)
        const container = document.getElementById("location-container");
        if (!container) return;

        const { PlaceAutocompleteElement } = await google.maps.importLibrary(
          "places"
        );

        locationAutocomplete = new PlaceAutocompleteElement({
          componentRestrictions: { country: "it" },
        });
        locationAutocomplete.id = "location-autocomplete";
        locationAutocomplete.placeholder = "Inserisci indirizzo di pick-up";
        locationAutocomplete.className = "form-control";

        container.appendChild(locationAutocomplete);

        locationAutocomplete.addEventListener(
          "gmp-select",
          async ({ placePrediction }) =>
            await onPlaceChangedAsync(placePrediction)
        );
      };

      const onPlaceChangedAsync = async (placePrediction) => {
        if (!placePrediction) {
          state.location = null;
          return;
        }

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
          state.location = null;
          return;
        }

        state.location = {
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

      // TODO share this with create-travel.js
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
        if (!isLocationValid.value) return null;
        return { location: state.location };
      };

      const handleRequestPickUp = async () => {
        const searchData = validateAndGetData();
        if (!searchData) {
          utilities.alertError(
            "Seleziona il pick-up point dai suggerimenti",
            3000
          );
          return;
        }

        const payload = {
          PickUpRequestId: 0,
          TravelId: routeData.travelId,
          Location: {
            LocationId: 0,
            ReadableAddress: searchData.location.formattedAddress,
            Coordinates: {
              Latitude: searchData.location.coordinates.lat,
              Longitude: searchData.location.coordinates.lng,
            },
            Street: searchData.location.addressComponents.route,
            Number: searchData.location.addressComponents.street_number,
            City: searchData.location.addressComponents.city,
            PostalCode: searchData.location.addressComponents.postalCode,
            Province: searchData.location.addressComponents.province,
            Region: searchData.location.addressComponents.region,
            Country: searchData.location.addressComponents.country,
            Continent: searchData.location.addressComponents.continent,
          },
        };

        try {
          const result = await utilities.postJsonT(
            "/Travel/EditPickUpRequest",
            payload
          );
          if (result && result.isSuccess) {
            utilities.alertSuccess("Richiesta inviata con successo!", 3000);
          } else {
            utilities.alertError("Errore nell'invio della richiesta", 3000);
          }
        } catch (err) {
          console.error("Edit pick-up request error:", err);
          utilities.alertError("Errore di connessione. Riprova.", 3000);
        }
      };

      // -- WATCHERS --
      watch(
        () => state.location,
        (newValue) => {
          if (locationAutocomplete) {
            locationAutocomplete.classList.toggle("is-valid", !!newValue);
            locationAutocomplete.classList.remove("is-invalid");
          }
        }
      );

      watch(
        () => state.submitted,
        (isSubmitted) => {
          if (isSubmitted && !isLocationValid.value && locationAutocomplete) {
            locationAutocomplete.classList.add("is-invalid");
          }
        }
      );

      // -- LIFECYCLE HOOK --
      onMounted(() => {
        initMap();
        initGooglePlaces();
      });

      return {
        state,
        isLocationValid,
        handleRequestPickUp,
      };
    },
  }).mount("#travelApp");
}
