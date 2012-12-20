﻿using System;

namespace OpenBve {
	internal static class ExtensionsCfgParser {

		// parse extensions config
		internal static void ParseExtensionsConfig(string TrainPath, System.Text.Encoding Encoding, out ObjectManager.UnifiedObject[] CarObjects, TrainManager.Train Train) {
			CarObjects = new ObjectManager.UnifiedObject[Train.Cars.Length];
			bool[] CarObjectsReversed = new bool[Train.Cars.Length];
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string FileName = OpenBveApi.Path.CombineFile(TrainPath, "extensions.cfg");
			if (System.IO.File.Exists(FileName)) {
				// load file
				string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
				for (int i = 0; i < Lines.Length; i++) {
					int j = Lines[i].IndexOf(';');
					if (j >= 0) {
						Lines[i] = Lines[i].Substring(0, j).Trim();
					} else {
						Lines[i] = Lines[i].Trim();
					}
				}
				for (int i = 0; i < Lines.Length; i++) {
					if (Lines[i].Length != 0) {
						switch (Lines[i].ToLowerInvariant()) {
							case "[exterior]":
								// exterior
								i++;
								while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
									if (Lines[i].Length != 0) {
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j >= 0) {
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											int n;
											if (int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out n)) {
												if (n >= 0 & n < Train.Cars.Length) {
													if (Interface.ContainsInvalidPathChars(b)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} else {
														string File = OpenBveApi.Path.CombineFile(TrainPath, b);
														if (System.IO.File.Exists(File)) {
															CarObjects[n] = ObjectManager.LoadObject(File, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
														} else {
															Interface.AddMessage(Interface.MessageType.Error, true, "The car object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													}
												} else {
													Interface.AddMessage(Interface.MessageType.Error, false, "The car index " + a + " does not reference an existing car at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												}
											} else {
												Interface.AddMessage(Interface.MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											}
										} else {
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									}
									i++;
								}
								i--;
								break;
							default:
								if (Lines[i].StartsWith("[car", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
									// car
									string t = Lines[i].Substring(4, Lines[i].Length - 5);
									int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n)) {
										if (n >= 0 & n < Train.Cars.Length) {
											bool DefinedLength = false;
											bool DefinedAxles = false;
											i++;
											while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
												if (Lines[i].Length != 0) {
													int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
													if (j >= 0) {
														string a = Lines[i].Substring(0, j).TrimEnd();
														string b = Lines[i].Substring(j + 1).TrimStart();
														switch (a.ToLowerInvariant()) {
															case "object":
																if (Interface.ContainsInvalidPathChars(b)) {
																	Interface.AddMessage(Interface.MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																} else {
																	string File = OpenBveApi.Path.CombineFile(TrainPath, b);
																	if (System.IO.File.Exists(File)) {
																		CarObjects[n] = ObjectManager.LoadObject(File, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
																	} else {
																		Interface.AddMessage(Interface.MessageType.Error, true, "The car object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "length":
																{
																	double m;
																	if (double.TryParse(b, System.Globalization.NumberStyles.Float, Culture, out m)) {
																		if (m > 0.0) {
																			Train.Cars[n].Length = m;
																			Train.Cars[n].BeaconReceiverPosition = 0.5 * m;
																			DefinedLength = true;
																		} else {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be a positive floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		}
																	} else {
																		Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be a positive floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "axles":
																{
																	int k = b.IndexOf(',');
																	if (k >= 0) {
																		string c = b.Substring(0, k).TrimEnd();
																		string d = b.Substring(k + 1).TrimStart();
																		double rear, front;
																		if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out rear)) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Rear is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out front)) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Front is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (rear >= front) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Rear is expected to be less than Front in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else {
																			Train.Cars[n].RearAxlePosition = rear;
																			Train.Cars[n].FrontAxlePosition = front;
																			DefinedAxles = true;
																		}
																	} else {
																		Interface.AddMessage(Interface.MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "reversed":
																CarObjectsReversed[n] = b.Equals("true", StringComparison.OrdinalIgnoreCase);
																break;
															default:
																Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																break;
														}
													} else {
														Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												}
												i++;
											}
											i--;
											if (DefinedLength & !DefinedAxles) {
												double AxleDistance = 0.4 * Train.Cars[n].Length;
												Train.Cars[n].RearAxlePosition = -AxleDistance;
												Train.Cars[n].FrontAxlePosition = AxleDistance;
											}
										} else {
											Interface.AddMessage(Interface.MessageType.Error, false, "The car index " + t + " does not reference an existing car at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								} else if (Lines[i].StartsWith("[coupler", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
									// coupler
									string t = Lines[i].Substring(8, Lines[i].Length - 9);
									int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n)) {
										if (n >= 0 & n < Train.Couplers.Length) {
											i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
												if (Lines[i].Length != 0) {
													int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
													if (j >= 0) {
														string a = Lines[i].Substring(0, j).TrimEnd();
														string b = Lines[i].Substring(j + 1).TrimStart();
														switch (a.ToLowerInvariant()) {
															case "distances":
																{
																	int k = b.IndexOf(',');
																	if (k >= 0) {
																		string c = b.Substring(0, k).TrimEnd();
																		string d = b.Substring(k + 1).TrimStart();
																		double min, max;
																		if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out min)) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Minimum is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out max)) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Maximum is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (min > max) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Minimum is expected to be less than Maximum in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else {
																			Train.Couplers[n].MinimumDistanceBetweenCars = min;
																			Train.Couplers[n].MaximumDistanceBetweenCars = max;
																		}
																	} else {
																		Interface.AddMessage(Interface.MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																} break;
															default:
																Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																break;
														}
													} else {
														Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												} i++;
											} i--;
										} else {
											Interface.AddMessage(Interface.MessageType.Error, false, "The coupler index " + t + " does not reference an existing coupler at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "The coupler index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								} else {
									// default
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								break;
						}
					}
				}
				// check for car objects and reverse if necessary
				int carObjects = 0;
				for (int i = 0; i < Train.Cars.Length; i++) {
					if (CarObjects[i] != null) {
						carObjects++;
						if (CarObjectsReversed[i]) {
							{
								// reverse axle positions
								double temp = Train.Cars[i].FrontAxlePosition;
								Train.Cars[i].FrontAxlePosition = -Train.Cars[i].RearAxlePosition;
								Train.Cars[i].RearAxlePosition = -temp;
							}
							if (CarObjects[i] is ObjectManager.StaticObject) {
								ObjectManager.StaticObject obj = (ObjectManager.StaticObject)CarObjects[i];
								CsvB3dObjectParser.ApplyScale(obj, -1.0, 1.0, -1.0);
							} else if (CarObjects[i] is ObjectManager.AnimatedObjectCollection) {
								ObjectManager.AnimatedObjectCollection obj = (ObjectManager.AnimatedObjectCollection)CarObjects[i];
								for (int j = 0; j < obj.Objects.Length; j++) {
									for (int h = 0; h < obj.Objects[j].States.Length; h++) {
										CsvB3dObjectParser.ApplyScale(obj.Objects[j].States[h].Object, -1.0, 1.0, -1.0);
										obj.Objects[j].States[h].Position.X *= -1.0;
										obj.Objects[j].States[h].Position.Z *= -1.0;
									}
									obj.Objects[j].TranslateXDirection.X *= -1.0;
									obj.Objects[j].TranslateXDirection.Z *= -1.0;
									obj.Objects[j].TranslateYDirection.X *= -1.0;
									obj.Objects[j].TranslateYDirection.Z *= -1.0;
									obj.Objects[j].TranslateZDirection.X *= -1.0;
									obj.Objects[j].TranslateZDirection.Z *= -1.0;
								}
							} else {
								throw new NotImplementedException();
							}
						}
					}
				}
				if (carObjects > 0 & carObjects < Train.Cars.Length) {
					Interface.AddMessage(Interface.MessageType.Warning, false, "An incomplete set of exterior objects was provided in file " + FileName);
				}
			}
		}

	}
}