terraform {
  required_version = "1.15.6"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
    tls = {
      source  = "hashicorp/tls"
      version = "~> 4.0"
    }
    local = {
      source  = "hashicorp/local"
      version = "2.9.0"
    }
  }
  backend "s3" {
  }
}

provider "aws" {
  region = var.aws_region
}

resource "tls_private_key" "nb-keypair" {
  algorithm = "RSA"
  rsa_bits  = 4096
}

resource "local_file" "private_key" {
  content  = tls_private_key.nb-keypair.private_key_pem
  filename = "${path.root}/nb-key-pair.pem"
}

resource "aws_key_pair" "nb-keypair" {
  key_name   = "nb-key-pair"
  public_key = tls_private_key.nb-keypair.public_key_openssh
}


resource "aws_instance" "demo-instance" {
  depends_on                  = [aws_security_group.allow_ssh, aws_subnet.public_subnet]
  ami                         = var.aws_ami_image
  instance_type               = var.aws_instance_type
  key_name                    = aws_key_pair.nb-keypair.key_name
  vpc_security_group_ids      = [aws_security_group.allow_ssh.id]
  subnet_id                   = aws_subnet.public_subnet.id
  associate_public_ip_address = true
  tags = {
    Name = "Mano ec2uske"
  }
}

resource "aws_vpc" "main_vpc" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_support   = true
  enable_dns_hostnames = true
}

resource "aws_subnet" "public_subnet" {
  vpc_id                  = aws_vpc.main_vpc.id
  cidr_block              = var.public_subnet_cidr
  availability_zone       = "eu-west-1a"
  map_public_ip_on_launch = true
}

resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.main_vpc.id
}

resource "aws_route_table" "public_rt" {
  vpc_id = aws_vpc.main_vpc.id
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
  }
}

resource "aws_route_table_association" "public_rt_association" {
  subnet_id      = aws_subnet.public_subnet.id
  route_table_id = aws_route_table.public_rt.id
}

resource "aws_security_group" "allow_ssh" {
  name        = "allow_ssh"
  description = "Allow ssh inbound"
  vpc_id      = aws_vpc.main_vpc.id
  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  lifecycle {
    create_before_destroy = true
  }
}

resource "local_file" "ansible_hosts" {
  content  = "[webservers]\n${aws_instance.demo-instance.public_ip}\n"
  filename = "${path.root}/ansible/ansible_hosts"
}

resource "aws_lb_target_group" "single_instance_tg" {
  name     = "single-instance-tg"
  port     = 80
  protocol = "HTTP"
  vpc_id   = aws_vpc.main_vpc.id
}

resource "aws_lb_target_group_attachment" "single_instance_tg_attachment" {
  target_group_arn = aws_lb_target_group.single_instance_tg.arn
  target_id        = aws_instance.demo-instance.id
  port             = 80
}

resource "aws_lb" "main_alb" {
  depends_on         = [aws_subnet.public_subnet, aws_subnet.public_subnet2]
  name               = "main-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.allow_ssh.id]
  subnets            = [aws_subnet.public_subnet.id, aws_subnet.public_subnet2.id]
}

resource "aws_lb_listener" "main_lb_listener" {
  load_balancer_arn = aws_lb.main_alb.arn
  port              = "80"
  protocol          = "HTTP"
  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.single_instance_tg.arn
  }
}

resource "aws_lb_listener" "https_lb_listener" {
  load_balancer_arn = aws_lb.main_alb.arn
  port              = 443
  protocol          = "HTTPS"
  certificate_arn   = aws_acm_certificate.chess_acm_cert.arn
  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.single_instance_tg.arn
  }
}

resource "aws_route53_zone" "chess_route53_zone" {
  name = var.domain_name
}

resource "aws_acm_certificate" "chess_acm_cert" {
  domain_name       = var.domain_name
  validation_method = "DNS"
  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_subnet" "public_subnet2" {
  vpc_id                  = aws_vpc.main_vpc.id
  cidr_block              = var.public_subnet2_cidr
  availability_zone       = "eu-west-1b"
  map_public_ip_on_launch = true
}

resource "aws_route53_record" "chess_validation_record" {
  zone_id = aws_route53_zone.chess_route53_zone.id
  name    = tolist(aws_acm_certificate.chess_acm_cert.domain_validation_options)[0].resource_record_name
  type    = tolist(aws_acm_certificate.chess_acm_cert.domain_validation_options)[0].resource_record_type
  records = [tolist(aws_acm_certificate.chess_acm_cert.domain_validation_options)[0].resource_record_value]
  ttl     = 60
}

resource "aws_acm_certificate_validation" "certificate_validation" {
  certificate_arn         = aws_acm_certificate.chess_acm_cert.arn
  validation_record_fqdns = [aws_route53_record.chess_validation_record.fqdn]
}

resource "aws_route53_record" "alb_alias_record" {
  zone_id = aws_route53_zone.chess_route53_zone.id
  name    = var.domain_name
  type    = "A"

  alias {
    name                   = aws_lb.main_alb.dns_name
    zone_id                = aws_lb.main_alb.zone_id
    evaluate_target_health = true
  }
}
