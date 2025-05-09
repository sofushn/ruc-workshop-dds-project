import { check, group, sleep } from "k6";
import http from "k6/http";

export const options = {
    stages: [
        { duration: "1m", target: 1000 },
        { duration: "30s", target: 0 },
    ],
    thresholds: {
        "http_req_duration": ["p(95)<250"],
        "checks": ["rate>0.95"], 
    },

};
export default function () {
    group("/images", () => {
        const response = http.get("http://localhost:8080/image-api/images/");

        check(response, {
            "status is 200": (r) => r.status === 200,
        });

    sleep(Math.random() * 4 + 1);
    });

    group("/3.jpg", () => {
        const response = http.get("http://localhost:8080/image-api/images/3.jpg");
        check(response, {
            "status is 200": (r) => r.status === 200,
        });

    sleep(Math.random() * 4 + 1);
    });

}