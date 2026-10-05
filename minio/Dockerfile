FROM golang:1.24-alpine AS build
ARG MINIO_RELEASE=RELEASE.2025-10-15T17-29-55Z
RUN apk add --no-cache git
RUN git clone --depth 1 --branch ${MINIO_RELEASE} https://github.com/minio/minio /src
WORKDIR /src
RUN CGO_ENABLED=0 go build -trimpath -o /out/minio .

FROM alpine:3.20
RUN apk add --no-cache ca-certificates
COPY --from=build /out/minio /usr/bin/minio
EXPOSE 9000 9001
ENTRYPOINT ["minio"]
