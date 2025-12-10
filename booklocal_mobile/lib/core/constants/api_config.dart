import 'dart:io';
import 'package:flutter/foundation.dart';

class ApiConfig {
  static String get baseUrl {
    if (kReleaseMode) {
      return 'https://apka-na-azure.net/api'; //produkcja
    }

    if (Platform.isAndroid) {
      return 'http://10.0.2.2:5000/api';
    } else {
      return 'http://localhost:5000/api';
    }
  }
}