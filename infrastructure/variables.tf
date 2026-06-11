variable "aws_key_name" {
  description = "The name of the AWS Key Pair to use"
  type        = string
  default     = "nb-key-pair"
}

variable "aws_ami_image" {
  description = "The AWS AMI to use"
  type        = string
  default     = "ami-00d582e3817d1a3da"
}

variable "aws_region" {
  description = "The AWS region to deploy resources"
  type        = string
  default     = "eu-west-1"
}

variable "aws_instance_type" {
  description = "The AWS instance type to use"
  type        = string
  default     = "t4g.small"
}

variable "public_subnet_cidr" {
  description = "cidr of the local subnet"
  type        = string
  default     = "10.0.0.0/24"
}

variable "domain_name" {
  description = "name of custom domain"
  type        = string
  default     = "chess-exerciser.tech"
}

variable "public_subnet2_cidr" {
  description = "cidr of the local subnet"
  type        = string
  default     = "10.0.1.0/24"
}
