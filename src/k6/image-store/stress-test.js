import { check, group } from "k6";
import http from "k6/http";
import exec from "k6/execution";

import { isStatusCode, isResponseUrlList, isResponseImage } from "../helpers/checkUtils.js";

const fileData = open("Trollface.jpg", "b");
const imageId = "e51d3639-b625-43c5-b122-15f39c0cb868.jpg";

export const options = {
    scenarios: {
        stress_test: {
        executor: "ramping-arrival-rate", 
        preAllocatedVUs: 50,
        maxVUs: 10000,
        stages: [
            { duration: "2m", target: 500 }, 
            { duration: "5m", target: 500 }, 
            { duration: "2m", target: 0 },
        ],
    },
    },
    thresholds: {
        "http_req_duration": [
            { threshold: "p(95)<800", abortOnFail: true, delayAbortEval: "20s" },
            { threshold: "avg < 500", abortOnFail: true, delayAbortVal: "20s" }
        ], 
        "http_req_failed": [{ threshold: "rate<0.01", abortOnFail: true, delayAbortVal: "20s" }],

        "checks": ["rate>0.95"], 
    },

};


export default function () {

    const iterationCount = exec.scenario.iterationInTest;
    group("GetAllImages", () => {
        const response = http.get("http://localhost:8080/image-api/images");

        check(response, {
            "status is 200": (r) => isStatusCode(r, 200),
            "response time < 800ms": (r) => r.timings.duration < 800, //Same as http_req_duration, but for this specific request
            "response is a list of URLs": (r) => isResponseUrlList(r),
        });
    });

    group("GetImageById", () => {
        const response = http.get(`http://localhost:8080/image-api/images/${imageId}`);
        check(response, {
            "status is 200": (r) => isStatusCode(r, 200),
            "response time < 800ms": (r) => r.timings.duration < 800,
            "response is an image": (r) => isResponseImage(r),
        });
    });

    if (iterationCount % 10000 === 0) {
        
    group("PostImg", () => {
        const data = {
            file: http.file(fileData, "Trollface.jpg"),
        };

        const response = http.post("http://localhost:5000/image-api/images", data);
        check(response, {
            "status is 201": (r) => isStatusCode(r, 201),
            "response time < 1000ms": (r) => r.timings.duration < 1000,
        });
   
    });
    };
}
