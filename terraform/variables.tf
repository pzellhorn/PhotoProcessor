variable "minio_server" {
  description = "MinIO endpoint in host:port form (the S3 API port). Use 127.0.0.1 rather than localhost so the client doesn't resolve to IPv6 ::1, which Docker's IPv4 port publish won't answer."
  type        = string
  default     = "127.0.0.1:9000"
}

variable "minio_user" {
  description = "MinIO access key."
  type        = string
  default     = "minioadmin"
}

variable "minio_password" {
  description = "MinIO secret key."
  type        = string
  default     = "minioadmin"
  sensitive   = true
}

variable "media_bucket" {
  description = "Bucket the API and workers read/write media through."
  type        = string
  default     = "media"
}
