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
        routeStats: {
          distanceText: null,
          durationText: null,
          visible: false,
        },
        location: null,
        submitted: false,
        pickUpRequestId: 0,
        pickUpRequestStatus: null,
        currentAddress: null, // For display purposes when editing
      });

      // Initialize from existing pickup request if available
      if (window.routeData && window.routeData.currentUserPickUpRequest) {
        const existingRequest = window.routeData.currentUserPickUpRequest;
        state.pickUpRequestId = existingRequest.pickUpRequestId;
        state.pickUpRequestStatus = existingRequest.status;
        state.currentAddress = existingRequest.location.formattedAddress;
        // Keep existing location data for when user doesn't change address
        state.location = {
          placeId: null,
          formattedAddress: existingRequest.location.formattedAddress,
          name: existingRequest.location.formattedAddress,
          coordinates: existingRequest.location.coordinates,
          addressComponents: existingRequest.location.addressComponents,
        };
      }

      let map = null;
      let locationAutocomplete = null;

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
        const container = document.getElementById("location-container");
        if (!container) return;

        // Import library
        const { PlaceAutocompleteElement } = await google.maps.importLibrary(
          "places"
        );

        // Create the element
        locationAutocomplete = new PlaceAutocompleteElement({
          componentRestrictions: { country: "it" },
        });

        locationAutocomplete.id = "location-autocomplete";
        locationAutocomplete.className = "form-control bg-pmu-dark";
        locationAutocomplete.placeholder =
          state.pickUpRequestId > 0
            ? "Inserisci nuovo indirizzo per modificare"
            : "Inserisci indirizzo di pick-up";

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

        const addressComponents = extractAddressComponents(
          place.addressComponents
        );

        state.location = {
          placeId: place.id,
          formattedAddress: place.formattedAddress,
          name: place.displayName,
          coordinates: {
            lat: place.location.lat(),
            lng: place.location.lng(),
          },
          addressComponents: addressComponents,
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

      // -- METHODS: FORM SUBMISSION --
      const handleRequestPickUp = async () => {
        state.submitted = true;

        if (!state.location) {
          utilities.alertError("Seleziona un indirizzo dai suggerimenti", 3000);
          return;
        }

        const travelId = window.routeData.travelId;
        const isEditing = state.pickUpRequestId > 0;
        const payload = {
          TravelId: travelId,
          PickUpRequestId: state.pickUpRequestId,
          Status:
            state.pickUpRequestStatus !== null ? state.pickUpRequestStatus : 0, // Maintain status when editing, Pending when creating
          Location: {
            LocationId: 0,
            ReadableAddress: state.location.formattedAddress,
            Coordinates: {
              Latitude: state.location.coordinates.lat,
              Longitude: state.location.coordinates.lng,
            },
            Street: state.location.addressComponents.route,
            Number: state.location.addressComponents.street_number,
            City: state.location.addressComponents.city,
            PostalCode: state.location.addressComponents.postalCode,
            Province: state.location.addressComponents.province,
            Region: state.location.addressComponents.region,
            Country: state.location.addressComponents.country,
            Continent: state.location.addressComponents.continent,
          },
        };

        try {
          const result = await utilities.postJsonT(
            "/Travel/EditPickUpRequest",
            payload
          );

          if (result && result.isSuccess) {
            const successMsg = isEditing
              ? "Richiesta di pick-up aggiornata!"
              : "Richiesta di pick-up inviata!";
            utilities.alertSuccess(successMsg, 2500);
            window.location.href = `/Travel/Travel?travelId=${travelId}`;
          } else {
            const msg =
              (result && result.errorMessage) ||
              "Errore durante l'invio della richiesta";
            utilities.alertError(msg, 3000);
          }
        } catch (error) {
          console.error("Error submitting pickup request:", error);
          utilities.alertError("Errore di connessione. Riprova.", 3000);
        }
      };

      // -- METHODS: DELETE --
      const handleDeletePickUpRequest = async () => {
        if (
          !confirm("Sei sicuro di voler eliminare la tua richiesta di pick-up?")
        ) {
          return;
        }

        const travelId = window.routeData.travelId;
        const formData = new FormData();
        formData.append("travelId", travelId);
        formData.append("pickUpRequestId", state.pickUpRequestId);

        try {
          const response = await fetch("/Travel/DeletePickUpRequest", {
            method: "POST",
            body: formData,
          });

          if (response.ok) {
            utilities.alertSuccess("Richiesta eliminata con successo!", 2500);
            window.location.href = `/Travel/Travel?travelId=${travelId}`;
          } else {
            utilities.alertError("Errore durante l'eliminazione", 3000);
          }
        } catch (error) {
          console.error("Error deleting pickup request:", error);
          utilities.alertError("Errore di connessione. Riprova.", 3000);
        }
      };

      // -- COMPUTED PROPERTIES --
      const isEditing = computed(() => state.pickUpRequestId > 0);
      const canEdit = computed(
        () =>
          state.pickUpRequestStatus === null || state.pickUpRequestStatus === 0
      );
      const canDelete = computed(() => state.pickUpRequestId > 0);
      const isPending = computed(() => state.pickUpRequestStatus === 0);
      const isAccepted = computed(() => state.pickUpRequestStatus === 1);

      // -- LIFECYCLE HOOK --
      onMounted(() => {
        initMap();
        initGooglePlaces();
      });

      return {
        state,
        handleRequestPickUp,
        handleDeletePickUpRequest,
        isEditing,
        canEdit,
        canDelete,
        isPending,
        isAccepted,
      };
    },
  }).mount("#travelApp");
}
