import { check, group } from "k6";
import http from "k6/http";

let getCount = 0;
const fileData = open("Trollface.jpg", "b");

export const options = {
    executor: "ramping-arrival-rate", 
    stages: [
        { duration: "2h", target: 10000 },
    ],
    gracefulStop: "1m",

    thresholds: {
        "http_req_duration": [
            { threshold: "p(90)<500", abortOnFail: true, delayAbortVal: "20s" },
            { threshold: "p(95)<800", abortOnFail: true, delayAbortEval: "20s" },
        ], 
        "http_req_failed": [{ threshold: "rate<0.01", abortOnFail: true, delayAbortVal: "20s" }],
        // "checks": ["rate>0.90"], 
    },

};

export default function () {
    group("/images", () => {
        const response = http.get("http://localhost:8080/image-api/images");

        check(response, {
            "status is 200": (r) => r.status === 200,
        });
        getCount++;
    });

    group("/3.jpg", () => {
        const response = http.get("http://localhost:8080/image-api/images/637d41bf-24e1-4ba5-87ef-33007f188316.jpg");
        check(response, {
            "status is 200": (r) => r.status === 200,
        });
        getCount++;
    });

    if (getCount > 1000) {
    group("PostImg", () => {
        const data = {
            file: http.file(fileData, "Trollface.jpg"),
        };

        const response = http.post("http://localhost:5000/image-api/images", data);
        check(response, {
            "status is 201": (r) => r.status === 201,
        });
        getCount = 0;
    });
    };
}