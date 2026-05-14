import http from "k6/http";
import { check, sleep } from "k6";

const BASE_URL = __ENV.BASE_URL || "http://localhost:5000";
const JAN = __ENV.JAN || "4901234567890";

export const options = {
  scenarios: {
    steady_load: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "30s", target: 5 },
        { duration: "60s", target: 20 },
        { duration: "30s", target: 0 }
      ],
      gracefulRampDown: "10s"
    }
  },
  thresholds: {
    http_req_failed: ["rate<0.05"],
    http_req_duration: ["p(95)<2000"],
    checks: ["rate>0.95"]
  }
};

export default function () {
  const url = `${BASE_URL}/api/products/${JAN}`;
  const response = http.get(url, {
    headers: {
      Accept: "application/json"
    },
    timeout: "5s"
  });

  const okStatus = response.status === 200 || response.status === 404 || response.status === 400;
  check(response, {
    "status is acceptable": () => okStatus,
    "response time < 2s": () => response.timings.duration < 2000
  });

  if (response.status === 200) {
    const body = response.json();
    check(body, {
      "has name": (b) => typeof b?.name === "string" && b.name.length > 0,
      "has manufacturer field": (b) => Object.prototype.hasOwnProperty.call(b, "manufacturer"),
      "has price field": (b) => Object.prototype.hasOwnProperty.call(b, "price")
    });
  }

  sleep(0.2);
}
