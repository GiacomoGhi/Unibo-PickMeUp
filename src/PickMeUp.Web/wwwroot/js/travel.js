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
}
