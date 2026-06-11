output "public_ip" {
  value = aws_instance.demo-instance.public_ip
}

output "ssh_command" {
  value     = "ssh -i ${path.root}/nb-key-pair.pem ubuntu@${aws_instance.demo-instance.public_ip}"
  sensitive = true
}
