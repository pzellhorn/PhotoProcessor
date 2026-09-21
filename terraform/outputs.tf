output "media_bucket" {
  description = "Name of the provisioned media bucket."
  value       = minio_s3_bucket.media.bucket
}
