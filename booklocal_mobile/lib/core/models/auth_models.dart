class LoginDto {
  final String email;
  final String password;

  LoginDto({required this.email, required this.password});

  Map<String, dynamic> toJson() {
    return {
      'email': email,
      'password': password,
    };
  }
}

class UserDto {
  final String id;
  final String email;
  final String firstName;
  final String lastName;
  final String? photoUrl;
  final List<String> roles;

  UserDto({
    required this.id,
    required this.email,
    required this.firstName,
    required this.lastName,
    this.photoUrl,
    required this.roles
  });

  factory UserDto.fromJson(Map<String, dynamic> json){
    return UserDto(
      id: json['id'] ?? '',
      email: json['email'] ?? '',
      firstName: json['firstName'] ?? '',
      lastName: json['lastName'] ?? '',
      photoUrl: json['photoUrl'],
      roles: List<String>.from(json['roles'] ?? []),
    );
  }

  bool get isOwner => roles.contains('owner');
  bool get isCustomer => roles.contains('customer');
}

class AuthResponseDto {
  final String token;
  final UserDto user;

  AuthResponseDto({required this.token, required this.user});

  factory AuthResponseDto.fromJson(Map<String, dynamic> json) {
    return AuthResponseDto(
      token: json['token'],
      user: UserDto.fromJson(json['user']),
    );
  }
}